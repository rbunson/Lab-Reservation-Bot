using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;


namespace LabReservationBot.Dialogs
{
    [Serializable]
    public class HelloDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            //Greet the user
            await context.PostAsync("Welcome to the lab! How are you?");

            //call context.Done
            context.Done<object>(null);
        }

    }
}
