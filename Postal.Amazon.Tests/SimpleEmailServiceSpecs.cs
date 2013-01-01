using System.Net.Mail;
using Amazon.SimpleEmail.Model;
using FakeItEasy;
using FluentAssertions;
using SubSpec;

namespace Postal.Amazon.Tests
{
    public class SimpleEmailServiceSpecs
    {
        [Specification]
        public void CanConvertMailMessageToSendEmailRequest()
        {
            var emailService = default(SimpleEmailService);
            var email = default(Email);
            var sendEmailRequest = default(SendEmailRequest);

            "Given I have an email".Context(() =>
                {
                    var fakeEmailRenderer = A.Fake<IEmailViewRenderer>();
                    var fakeEmailParser = A.Fake<IEmailParser>();
                    var fakeAmazonEmailService = A.Fake<global::Amazon.SimpleEmail.AmazonSimpleEmailService>();
                    email = new Email("test");
                    A.CallTo(() => fakeEmailRenderer.Render(email, null)).Returns("");
                    A.CallTo(() => fakeEmailParser.Parse("", email)).Returns(new MailMessage("sender@test.com","test@test.com","Subject","Body"));
                    A.CallTo(() => fakeAmazonEmailService.SendEmail(A<SendEmailRequest>.Ignored)).Invokes(x => sendEmailRequest = x.Arguments.Get<SendEmailRequest>(0))
                     .Returns(new SendEmailResponse());

                    emailService = new SimpleEmailService(fakeEmailRenderer, fakeEmailParser, fakeAmazonEmailService);
                });

            "When I create an Amazon SES Request".Do(() => emailService.Send(email));

            "Then the destination address is set".Observation(() => sendEmailRequest.Destination.ToAddresses[0].ShouldBeEquivalentTo("test@test.com"));

            "And the source address is set".Observation(() => sendEmailRequest.Source.ShouldBeEquivalentTo("sender@test.com"));

            "And the subject is set".Observation(() => sendEmailRequest.Message.Subject.Data.ShouldBeEquivalentTo("Subject"));

            "And the body is set".Observation(() => sendEmailRequest.Message.Body.Text.Data.ShouldBeEquivalentTo("Body"));
        }
    }
}
