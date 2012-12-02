using System.Net.Mail;
using Amazon.SimpleEmail.Model;
using FakeItEasy;
using SubSpec;
using Xunit;

namespace Postal.AmazonSES.Tests
{
    public class AmazonSimpleEmailServiceSpecs
    {
        [Specification]
        public void CanConvertMailMessageToSendEmailRequest()
        {
            var emailService = default(AmazonSimpleEmailService);
            var email = default(Email);
            var sendEmailRequest = default(SendEmailRequest);

            "Given I have an email".Context(() =>
                {
                    var fakeEmailRenderer = A.Fake<IEmailViewRenderer>();
                    var fakeEmailParser = A.Fake<IEmailParser>();
                    var fakeAmazonEmailService = A.Fake<Amazon.SimpleEmail.AmazonSimpleEmailService>();
                    email = new Email("test");
                    A.CallTo(() => fakeEmailRenderer.Render(email, null)).Returns("");
                    A.CallTo(() => fakeEmailParser.Parse("", email)).Returns(new MailMessage("sender@test.com","test@test.com","Subject","Body"));
                    A.CallTo(() => fakeAmazonEmailService.SendEmail(A<SendEmailRequest>.Ignored)).Invokes(x => sendEmailRequest = x.Arguments.Get<SendEmailRequest>(0))
                     .Returns(new SendEmailResponse());

                    emailService = new AmazonSimpleEmailService(fakeEmailRenderer, fakeEmailParser, fakeAmazonEmailService);
                });

            "When I create an Amazon SES Request".Do(() => emailService.Send(email));

            "Then the destination address is set".Observation(() => Assert.Equal("test@test.com", sendEmailRequest.Destination.ToAddresses[0]));

            "And the source address is set".Observation(() => Assert.Equal("sender@test.com", sendEmailRequest.Source));

            "And the subject is set".Observation(() => Assert.Equal("Subject", sendEmailRequest.Message.Subject.Data));

            "And the body is set".Observation(() => Assert.Equal("Body", sendEmailRequest.Message.Body.Text.Data));
        }
    }
}
