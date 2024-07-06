using System.Net.Mail;
using System.Net;
using GraduationProject.core.DataDTO;

namespace GrdauationProject.EF
{
    public class EmailModel
    {
        public string FromEmail { get; set; } = "sanaaelhaproty@gmail.com";
        public string ToEmails { get; set; } = "sana.20377387@compit.aun.edu.eg";
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string DoctorSubject { get; set; } = "Appointment Dates Not Suitable - Please Select New Dates";

        public EmailModel()
        {

        }
        public bool SendEmail()
        {
            var message = new MailMessage()
            {
                From = new MailAddress(this.FromEmail),
                Subject = this.Subject,
                IsBodyHtml = true,
                Body = this.Body
            };
            foreach (var toEmail in this.ToEmails.Split(";"))
            {
                message.To.Add(new MailAddress(toEmail));
            }

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(this.FromEmail, "insf darb tmqa lxcq"),
                EnableSsl = true,
            };
            try
            {
                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string DoctorMessage (InValidCourses obj)
        {
            string body = $"""
                <html>
                    <body>
                       <p>Dear Docotr <strong>{obj.DoctorName}</strong>,</p>
                       <p>We regret to inform you that the following dates you selected for your appointment are not available:</p>
                       <ul>
                          <li>{obj.Option1}</li>
                          <li>{obj.Option2}</li>
                          <li>{obj.Option3}</li>

                       </ul>
                       <p>Please choose new dates for your appointment at your earliest convenience.</p>
                       <p>Thank you for your understanding.</p>
                       <p>Best regards,</p>
                       <p><strong>FCI</strong></p>
                    </body>
                </html>
                """;
            return body;
        }
        public string CustomBody(string input)
        {
            string body = $"""
                <html>
                    <body>
                       <p><strong>{input}</strong>,</p>
                    </body>
                </html>
                """;
            return body;

        }
        public string SendEmailTo(string email, string subject, string body)
        {
            if (string.IsNullOrEmpty(email))
            {
                return ("Incorrect email");
            }
            EmailModel Email = new EmailModel();
            Email.ToEmails = email;
            Email.Subject = subject;
            Email.Body = body;
            if (Email.SendEmail())
                return ("Email Sent!");
            return ("Email Dosen't Sent!");
        }
    }
}
