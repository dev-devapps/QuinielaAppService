// Funciones.cs
using System;
using System.Net.Mail;

namespace QuinielaAPP
{
    public class Funciones
    {
        public Funciones()
        {
        }
    }

    public class SMTPClass
    {
        public String HOST { get; set; }
        public int PORT { get; set; }
        public String SMTP_USERNAME { get; set; }
        public String SMTP_PASSWORD { get; set; }
        public Boolean ENABLESSL { get; set; }

        public SMTPClass()
        {
            SMTP_USERNAME = "";
            SMTP_PASSWORD = "";
            ENABLESSL = false;
        }

        /// <summary>Método que envía un correo electrónico a un destinatario o destinatarios, teniendo la opción de enviar archivos adjuntos y/o código HTML.</summary>
        /// <param name="strDisplayName">Nombre de quien está enviando el correo electrónico.</param>
        /// <param name="strFrom">Acá se coloca el correo electrónico de quien está originando el correo</param>
        /// <param name="strTo">Correo electrónico lista de correos electrónicos a quienes se enviará el mensaje, para enviar a más de un destinatario, se debe de serparar por comas. 
        /// <example>Ejemplo: pepito@correo.com,mafalda@correo.com</example>
        /// </param>
        /// <param name="strCc">Listado de correos (separados por comas) a quienes se les enviará la copia.</param>
        /// <param name="strBcc">Listado de correos (separados por comas) a quienes se les enviará la copia oculta.</param>
        /// <param name="strSubject">Asunto del correo electrónico.</param>
        /// <param name="isHTML">Especifica se si enviará texto plano o código HTML, para texto plano enviar "false", para enviar código HTML se debe de enviar "true".</param>
        /// <param name="strBody">Contenido del correo eléctrónico.</param>
        /// <param name="strAttachmentPath">Ruta física en donde se encuentra el archivo, si no se desea adjuntar archivo enviar vacío. Para enviar más de un adjunto separar por comas las rutas.</param>
        public String SendMail(String strDisplayName, String strFrom, String strTo, String strCc, String strBcc, String strSubject, Boolean isHTML, String strBody, String strAttachmentPath)
        {
            String lResult = "";
            String strErrores = "";
            String strResultado = "";
            String strAttachments = "";
            String strAttachDetail = "";

            try
            {
                MailAddress mailFrom = new MailAddress(strFrom, strDisplayName);
                MailAddressCollection mailTo = new MailAddressCollection();
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                mailTo.Add(strTo);

                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(HOST, PORT);

                if (!SMTP_USERNAME.Equals(""))
                    client.Credentials = new System.Net.NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                client.EnableSsl = ENABLESSL;

                //Asignacion de variables
                message.IsBodyHtml = isHTML;
                message.From = mailFrom;

                message.To.Add(strTo);
                message.Subject = strSubject;
                message.Body = strBody;

                //Copia
                if (!strCc.Equals(""))
                {
                    message.CC.Add(strCc);
                }

                //Copia oculta
                if (!strBcc.Equals(""))
                {
                    message.Bcc.Add(strBcc);
                }

                if (!strAttachmentPath.Equals(""))
                {

                    String[] arrFileName;

                    arrFileName = strAttachmentPath.Split(',');

                    if (arrFileName.Length > 0)
                    {
                        for (int i = 0; i < arrFileName.Length; i++)
                        {
                            try
                            {
                                message.Attachments.Add(new Attachment(arrFileName[i]));

                                strAttachDetail += "<adjunto resultado=\"1\">" +
                                                        "<mensaje>OK</mensaje>" +
                                                        "<path><![CDATA[" + arrFileName[i] + "]]></path>" +
                                                   "</adjunto>";
                            }
                            catch (Exception e)
                            {
                                strErrores += e.Message + ": " + arrFileName[i] + "|";

                                strAttachDetail += "<adjunto resultado=\"2\">" +
                                                        "<mensaje><![CDATA[" + e.Message + "]]></mensaje>" +
                                                        "<path><![CDATA[" + arrFileName[i] + "]]></path>" +
                                                   "</adjunto>";
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Attachment adjunto = new Attachment(strAttachmentPath);
                            message.Attachments.Add(adjunto);

                            strAttachDetail += "<adjunto resultado=\"1\">" +
                                                    "<mensaje>OK</mensaje>" +
                                                    "<path><![CDATA[" + strAttachmentPath + "]]></path>" +
                                               "</adjunto>";
                        }
                        catch (Exception e2)
                        {
                            strErrores += e2.Message + ": " + strAttachmentPath + "|";

                            strAttachDetail += "<adjunto resultado=\"2\">" +
                                                    "<mensaje><![CDATA[" + e2.Message + "]]></mensaje>" +
                                                    "<path><![CDATA[" + strAttachmentPath + "]]></path>" +
                                               "</adjunto>";
                        }
                    }

                    strAttachments = "<adjuntos>" +
                                        strAttachDetail +
                                     "</adjuntos>";

                }
                else
                {
                    strAttachments = "<adjuntos/>";
                }

                client.Send(message);
                lResult = "1 - Email sent!";

                strResultado = "<envio_correo>" +
                                    "<resultado codigo=\"1\">Envio de correo exitoso.</resultado>" +
                                    strAttachments +
                               "</envio_correo>";
            }
            catch (Exception ex)
            {
                lResult = "2 - The email was not sent. Error message: " + ex.Message;

                strResultado = "<envio_correo>" +
                                    "<resultado codigo=\"2\"><![CDATA[" + ex.Message + "]]></resultado>" +
                               "</envio_correo>";
            }

            //return lResult;
            return strResultado;
        }
    }
}
