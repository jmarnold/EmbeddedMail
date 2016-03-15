using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace EmbeddedMail.Handlers
{
    public class MessageParser
    {
    public static readonly string Newline_XP = @"[\r\n]+";
         public MailMessage Parse(string data)
         {
             var result = new SmtpMessage(data);
             var parts = result.MessageParts;
             // sanity check
             if(!parts.Any())
             {
                 throw new InvalidOperationException("Invalid message body");
             }

             // use the first for the body
             var body = parts.First();
             var message = new MailMessage
                               {
                                   Body = body.BodyData.Replace("=\r\n", string.Empty), // holy crap this is dumb
                                   Subject = result.Headers["Subject"],
                                   From = new MailAddress(result.Headers["From"])
                               };

             var recipients = result.Headers["To"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
             recipients.Each(r => message.To.Add(new MailAddress(r)));

             if(result.Headers.ContainsKey("Cc"))
             {
                 var copies = result.Headers["Cc"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                 copies.Each(r => message.CC.Add(new MailAddress(r)));
             }

             var attachments = parts.Skip(1).ToArray();
             attachments.Each(x =>
            {
                var type = x.Headers["Content-Type"];
                var stream = new MemoryStream();
                var bytes = Encoding.UTF8.GetBytes(x.BodyData);
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                message.Attachments.Add(new Attachment(stream, new ContentType(type)));
            });

             return message;
         }
    }
}