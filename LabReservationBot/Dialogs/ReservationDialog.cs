﻿using System;
using Microsoft.Bot.Builder.FormFlow;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LabReservationBot.Dialogs
{
    [Serializable]
    public class ReservationDialog
    {

        public enum DivisionOptions
        {
            Reinsurance,
            Primary,
            Specialty,
            none
        }

        [Prompt(new string[] { "What is your name?" })]
        public string Name { get; set; }

        [Prompt(new string[] { "What is your email?" })]
        public string Email { get; set; }

        [Prompt("What date would you like to start your engagement? example: today, tomorrow, or any date like 04-06-2017 {||}", AllowDefault = BoolDefault.True)]
        [Describe("Reservation date, example: today, tomorrow, or any date like 04-06-2017")]
        public DateTime ReservationStartDate { get; set; }

        [Prompt("How many days will you need the lab?")]
        [Numeric(1, 20)]
        public int? NumberOfDays;

        [Prompt("How many people are in your project?")]
        [Numeric(1, 20)]
        public int? NumberOfParticipants;
        public DivisionOptions? Divisions;

        public static IForm<ReservationDialog> BuildForm()
        {
            return new FormBuilder<ReservationDialog>()
                .Field(nameof(Name))
                .Field(nameof(Email), validate: ValidateContactInformation)
                .Field(nameof(ReservationStartDate))
                .Field(nameof(NumberOfDays))
                .Field(nameof(Divisions))
                .AddRemainingFields()
                .Build();
        }

        private static Task<ValidateResult> ValidateContactInformation(ReservationDialog state, object response)
        {
            var result = new ValidateResult();
            string contactInfo = string.Empty;
            if (GetEmailAddress((string)response, out contactInfo))
            {
                result.IsValid = true;
                result.Value = contactInfo;
            }
            else
            {
                result.IsValid = false;
                result.Feedback = "You did not enter valid email address.";
            }
            return Task.FromResult(result);
        }

        private static bool GetEmailAddress(string response, out string contactInfo)
        {
            contactInfo = string.Empty;
            var match = Regex.Match(response, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            if (match.Success)
            {
                contactInfo = match.Value;
                return true;
            }
            return false;
        }

        private static PromptAttribute PerLinePromptAttribute(string pattern)
        {
            return new PromptAttribute(pattern)
            {
                ChoiceStyle = ChoiceStyleOptions.PerLine
            };
        }


    }
}