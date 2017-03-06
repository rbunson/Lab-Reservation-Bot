using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace LabReservationBot.Dialogs
{
    [Serializable]
    [LuisModel("8056b6bd-6e4a-470f-8a43-53290454d658", "9768f3f6bae042e89970ce89a6d9e32b")]
    public class RootDialog : LuisDialog<object>
    {
        private const string ReservationOption = "Reserve Space in Lab";
        private const string HelloOption        = "Say Hello"    ;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ReserveSpace")]
        public async Task ReserveSpace(IDialogContext context, LuisResult result)
        {
            try
            {
                await context.PostAsync("Great, lets get your project going. You will need to provide a few details.");
                var reservationForm = new FormDialog<ReservationDialog>(new ReservationDialog(), ReservationDialog.BuildForm, FormOptions.PromptInStart);
                context.Call(reservationForm, ReservationFormComplete);
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("SayHello")]
        public async Task SayHello(IDialogContext context, LuisResult result)
        {
            context.Call(new HelloDialog(), this.ResumeAfterOptionDialog);
        }
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Insert Help Dialog here");
            context.Wait(MessageReceived);
        }


        private async Task ResumeAfterUserInfoDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.Wait(this.MessageReceivedAsync);

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {


                PromptDialog.Choice(
                context,
                this.OnOptionSelected,
                // Present two (2) options to user
                new List<string>() { ReservationOption, HelloOption },
                String.Format("Hi are you looking for to reserve a space in the lab or Just say hello?"), "Not a valid option", 3);

        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                //capture which option then selected
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case ReservationOption:
                        context.Call(FormDialog.FromForm<ReservationDialog>(ReservationDialog.BuildForm,
                        FormOptions.PromptInStart), this.ReservationFormComplete);

                        break;

                    case HelloOption:
                        context.Call(new HelloDialog(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                //If too many attempts we send error to user and start all over. 
                await context.PostAsync($"Ooops! Too many attemps :( You can start again!");

                //This sets us in a waiting state, after running the prompt again. 
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ReservationFormComplete(IDialogContext context, IAwaitable<ReservationDialog> result)
        {
            try
            {
                var reservation = await result;
                await context.PostAsync("Thanks for the using Lab Reservation Bot.");
                //use a card for showing their data
                var resultMessage = context.MakeMessage();
                //resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                string ThankYouMessage;

                ThankYouMessage = reservation.Name + ", thank you for using the Lab, we look forward to having you and your team";

                var endDate = reservation.ReservationStartDate.AddDays(reservation.NumberOfDays.Value);
                ThumbnailCard thumbnailCard = new ThumbnailCard()
                {

                    Title = String.Format("Lab Reservations from {0} to {1}", reservation.ReservationStartDate.ToString("MM/dd/yyyy"), endDate.ToString ("MM/dd/yyyy")),
                    Subtitle = String.Format("For {0} people", reservation.NumberOfParticipants),
                    Text = ThankYouMessage,
                    Images = new List<CardImage>()
                {
                    new CardImage() { Url = "https://upload.wikimedia.org/wikipedia/en/e/ee/Unknown-person.gif" }
                },
                };

                resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                await context.PostAsync(resultMessage);
                await context.PostAsync(String.Format(""));
            }
            catch (FormCanceledException)
            {
                await context.PostAsync("You canceled the transaction, ok. ");
            }
            catch (Exception ex)
            {
                var exDetail = ex;
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
            }
            finally
            {
                context.Wait(MessageReceivedAsync);
            }
        }


        /// <summary>
        ///  User did not select a reservation. Loop back to original statement and ask if they would like to make one.
        /// </summary>
        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
               
                var message = await result;
                

            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }


    }
}