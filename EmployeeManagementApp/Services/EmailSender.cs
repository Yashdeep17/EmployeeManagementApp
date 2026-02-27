using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmployeeManagementApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // 1. Grab credentials from appsettings.json
            var apiKey = _configuration["EmailSettings:SendGridKey"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            // 2. Initialize the SendGrid Client
            var client = new SendGridClient(apiKey);

            // 3. Set up the From and To addresses
            var from = new EmailAddress(senderEmail, senderName);
            var to = new EmailAddress(email);

            // 4. Create the email payload
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            // 5. Fire off the email to SendGrid's API!
            var response = await client.SendEmailAsync(msg);

            // 6. If SendGrid rejects it, this will force our Try/Catch block to show us exactly why
            if (!response.IsSuccessStatusCode)
            {
                // This grabs the exact error message from SendGrid so we aren't guessing
                string errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid API failed. Status: {response.StatusCode}. Details: {errorBody}");
            }
        }
    }
}