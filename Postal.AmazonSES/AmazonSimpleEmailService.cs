using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Amazon.SimpleEmail.Model;

namespace Postal.AmazonSES
{
    public class AmazonSimpleEmailService : IEmailService
    {
        readonly IEmailViewRenderer _emailViewRenderer;
        readonly Amazon.SimpleEmail.AmazonSimpleEmailService _simpleEmailService;
        readonly IEmailParser _emailParser;

        public AmazonSimpleEmailService(IEmailViewRenderer emailViewRenderer)
        {
            _emailViewRenderer = emailViewRenderer;
            _simpleEmailService = Amazon.AWSClientFactory.CreateAmazonSimpleEmailServiceClient();
            _emailParser = new EmailParser(emailViewRenderer);
        }

        public AmazonSimpleEmailService(IEmailViewRenderer emailViewRenderer, Amazon.SimpleEmail.AmazonSimpleEmailService amazonSimpleEmailService)
        {
            _emailViewRenderer = emailViewRenderer;
            _simpleEmailService = amazonSimpleEmailService;
            _emailParser = new EmailParser(emailViewRenderer);
        }

        public AmazonSimpleEmailService(IEmailViewRenderer emailViewRenderer, IEmailParser emailParser, Amazon.SimpleEmail.AmazonSimpleEmailService amazonSimpleEmailService)
        {
            _emailViewRenderer = emailViewRenderer;
            _simpleEmailService = amazonSimpleEmailService;
            _emailParser = emailParser;
        }

        public void Send(Email email)
        {
            using (var mailMessage = CreateMailMessage(email))
            {
                _simpleEmailService.SendEmail(CreateSendEmailRequest(mailMessage));
            }
        }

        public Task SendAsync(Email email)
        {
            var taskCompletionSource = new TaskCompletionSource<SendEmailResult>();
            using (var mailMessage = CreateMailMessage(email))
            {
                _simpleEmailService.BeginSendEmail(CreateSendEmailRequest(mailMessage),
                    e =>
                        {
                            var result = _simpleEmailService.EndSendEmail(e);
                            taskCompletionSource.SetResult(result.SendEmailResult);
                    }, null);
            }
            return taskCompletionSource.Task;
        }

        public MailMessage CreateMailMessage(Email email)
        {
            var rawEmailString = _emailViewRenderer.Render(email);
            var mailMessage = _emailParser.Parse(rawEmailString, email);
            return mailMessage;
        }

        private static SendEmailRequest CreateSendEmailRequest(MailMessage mailMessage)
        {
            return new SendEmailRequest
                       {
                           Destination = new Destination(mailMessage.To.Select(e => e.Address).ToList()),
                           Source = mailMessage.From.Address,
                           Message = new Message
                                         {
                                             Body = new Body(new Content(mailMessage.Body)),
                                             Subject = new Content(mailMessage.Subject)
                                         }
                       };
        }
    }
}