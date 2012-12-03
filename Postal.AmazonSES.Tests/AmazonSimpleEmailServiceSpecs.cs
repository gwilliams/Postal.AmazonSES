using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Amazon.SimpleEmail.Model;
using FakeItEasy;
using FluentAssertions;
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

            "Then the destination address is set".Observation(() => sendEmailRequest.Destination.ToAddresses[0].ShouldBeEquivalentTo("test@test.com"));

            "And the source address is set".Observation(() => sendEmailRequest.Source.ShouldBeEquivalentTo("sender@test.com"));

            "And the subject is set".Observation(() => sendEmailRequest.Message.Subject.Data.ShouldBeEquivalentTo("Subject"));

            "And the body is set".Observation(() => sendEmailRequest.Message.Body.Text.Data.ShouldBeEquivalentTo("Body"));
        }

        [Specification]
        public void CanSendAsyncEmail()
        {
            var emailService = default(AmazonSimpleEmailService);
            var email = default(Email);
            var sendEmailRequest = default(SendEmailRequest);
            var result = default(object);

            "Given I have an email".Context(() =>
            {
                var fakeEmailRenderer = A.Fake<IEmailViewRenderer>();
                var fakeEmailParser = A.Fake<IEmailParser>();
                var fakeAmazonEmailService = A.Fake<Amazon.SimpleEmail.AmazonSimpleEmailService>();
                email = new Email("test");
                A.CallTo(() => fakeEmailRenderer.Render(email, null)).Returns("");
                A.CallTo(() => fakeEmailParser.Parse("", email)).Returns(new MailMessage("sender@test.com", "test@test.com", "Subject", "Body"));

                A.CallTo(
                    () =>
                    fakeAmazonEmailService.BeginSendEmail(A<SendEmailRequest>.Ignored,
                                                          (s) => A.CallTo(() => fakeAmazonEmailService.EndSendEmail(s))
                                                          .Returns(new SendEmailResponse
                                                                       {
                                                                           SendEmailResult = new SendEmailResult
                                                                                                 {
                                                                                                     MessageId = "123"
                                                                                                 }
                                                                       }),
                                                          null));


                emailService = new AmazonSimpleEmailService(fakeEmailRenderer, fakeEmailParser, fakeAmazonEmailService);
            });

            "When I create an Amazon SES Request".Do(() => emailService.SendAsync(email));

            "Then something".Observation(() => { var x = 1; });
        }
    }
}
