// Funciones.cs
using System;
using System.Collections.Generic;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


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

            InternetAddressList listaCorreo = new InternetAddressList();

            try
            {

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(strDisplayName, strFrom));

                //Lista de correo destinatario
                String[] cDestinatario = strTo.Split(',');

                if (cDestinatario.Length > 0)
                {
                    foreach (String mail in cDestinatario)
                    {
                        listaCorreo.Add(new MailboxAddress(mail, mail));
                    }
                }
                else
                {
                    listaCorreo.Add(new MailboxAddress(strTo, strTo));
                }

                message.To.AddRange(listaCorreo);

                //Lista de correo copia

                if (!strTo.Equals(""))
                {
                    String[] cCopia = strTo.Split(',');

                    if (cCopia.Length > 0)
                    {
                        foreach (String mail in cCopia)
                        {
                            listaCorreo.Add(new MailboxAddress(mail, mail));
                        }
                    }
                    else
                    {
                        listaCorreo.Add(new MailboxAddress(strCc, strCc));
                    }
                }
                

                message.Cc.AddRange(listaCorreo);

                //Lista de correo oculta

                if (!strBcc.Equals(""))
                {
                    String[] cOculta = strBcc.Split(',');

                    if (cOculta.Length > 0)
                    {
                        foreach (String mail in cOculta)
                        {
                            listaCorreo.Add(new MailboxAddress(mail, mail));
                        }
                    }
                    else
                    {
                        listaCorreo.Add(new MailboxAddress(strBcc, strBcc));
                    }
                    message.Bcc.AddRange(listaCorreo);
                }
                
                message.Subject = strSubject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = strBody;
                
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

                                byte[] dataF = File.ReadAllBytes(arrFileName[i]);

                                bodyBuilder.Attachments.Add(Path.GetFileName(arrFileName[i]), dataF);

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
                            byte[] dataF = File.ReadAllBytes(strAttachmentPath);

                            bodyBuilder.Attachments.Add(Path.GetFileName(strAttachmentPath), dataF);

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

                message.Body = bodyBuilder.ToMessageBody();

                var client = new SmtpClient();

                client.Connect(HOST, PORT, SecureSocketOptions.Auto);
                client.Authenticate(SMTP_USERNAME, SMTP_PASSWORD);

                client.Send(message);
                client.Disconnect(true);

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
