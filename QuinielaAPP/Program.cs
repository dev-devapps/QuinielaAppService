// Program.cs
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Xml;

namespace QuinielaAPP
{
    class MainClass
    {
        public const string SQLCONNSTRING = "SQLConnString";

        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando...");

            CambiaEstadoPartidos();

            ConsultaPartidoFinalizado();
        }


        private static void CambiaEstadoPartidos(){
            try
            {
                Console.Write("Actualizando partidos... ");
                // Build connection string
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

                // Connect to SQL
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    string query = "EXEC sp_cambia_estado_partido";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Actualizacion exitosa! :)");
                        Console.WriteLine(" ");
                    }

                    query = "select pa_id, E1.eq_descripcion 'Equipo1', E2.eq_descripcion 'Equipo1' from Partido, Equipo as E1, Equipo as E2 where pa_estado = 'C' and E1.eq_id = pa_idEquipo1 and E2.eq_id = pa_idEquipo2 and pa_hora_pronostico is null";

                    using (SqlCommand command2 = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command2.ExecuteReader())
                        {
                            int idPartido = 0;
                            string equipo1 = "", equipo2 = "", htmlCuerpoCorreo = "";


                            while (reader.Read())
                            {
                                idPartido = reader.GetInt32(0);
                                equipo1 = reader.GetString(1);
                                equipo2 = reader.GetString(2);

                                htmlCuerpoCorreo = ArmaHTMLPronostico(idPartido, equipo1, equipo2);

                                Console.WriteLine("Enviando pronosticos de: " + equipo1 + " vs. " + equipo2);
                                EnvioCorreo("Pronosticos " + equipo1 + " vs. " + equipo2, htmlCuerpoCorreo);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Ocurrio un error actualizando los partidos:" + e.ToString());
            }
        }

        private static string ArmaHTMLPronostico(int idPartido, string equipo1, string equipo2)
        {
            string htmlPronosticos = "", query = "", trClass = "";
            int cont = 0;

            string bg = ConfigurationManager.AppSettings["BackgroundColor"].ToString(), trPar = ConfigurationManager.AppSettings["trPar"].ToString(), trImpar = ConfigurationManager.AppSettings["trImpar"].ToString();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

            // Connect to SQL
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                query = "EXEC sp_tabla_pronostico @idPartido";


                using (SqlCommand cmPronostico = new SqlCommand(query, connection))
                {

                    cmPronostico.Parameters.AddWithValue("@idPartido", idPartido);

                    using(SqlDataReader rMarcadores = cmPronostico.ExecuteReader()){
                        htmlPronosticos = "<table align=\"center\" cellpadding=\"2\" cellspacing=\"2\" border=\"0\">" +
                      "<tr style=\"font-family:Verdana;font-size:11px;font-weight: bold;padding:0px 10px 0px 0px;color:#FFFFFF;background-color:#" + bg +";\">" +
                        "<td align=\"center\">Alias</td>" +
                        "<td align=\"center\">" + equipo1 + "</td>" +
                        "<td align=\"center\">" + equipo2 + "</td>" +
                        "<td align=\"center\">Ingreso Pron&oacute;stico</td>" +
                      "</tr>";

                        while (rMarcadores.Read())
                        {

                            if ((cont % 2) == 0)
                            {
                                trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#" + trPar + ";";
                            }
                            else
                            {
                                trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#" + trImpar + ";";
                            }

                            htmlPronosticos += "<tr style=\"" + trClass + "\">" +
                                                       "<td>" + rMarcadores.GetString(1) + "</td>" +
                                                       "<td align=\"center\">" + (rMarcadores.GetInt32(2) == -1 ? "-" : rMarcadores.GetInt32(2).ToString()) + "</td>" +
                                                       "<td align=\"center\">" + (rMarcadores.GetInt32(3) == -1 ? "-" : rMarcadores.GetInt32(3).ToString()) + "</td>" +
                                                       "<td align=\"center\">" + (rMarcadores.GetInt32(3) == -1 ? "-" : rMarcadores.GetString(4) + " " + rMarcadores.GetString(5)) + "</td>" +
                                                "</tr>";

                            cont++;
                        }

                        htmlPronosticos += "</table>";
                    }

                }

                try{
                    query = "update Partido set pa_hora_pronostico = GETDATE() where pa_id = @idPartido";

                    using (SqlCommand cmActualizaPronostico = new SqlCommand(query, connection))
                    {
                        cmActualizaPronostico.Parameters.AddWithValue("@idPartido", idPartido);

                        cmActualizaPronostico.ExecuteNonQuery();
                    }
                }catch(Exception e){
                    Console.WriteLine("Ocurrio un error: " + e.Message);
                }

            }


            htmlPronosticos += "<br /><br />Recuerda que puedes ingresar al portal haciendo clic <a href =\"" + ConfigurationManager.AppSettings["HOME"].ToString() + "\">aqu&iacute;</a>.";


            return htmlPronosticos;
        }

        private static void ConsultaPartidoFinalizado(){
            try
            {
                Console.Write("Verificando si existen partidos finalizados... ");
                // Build connection string
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

                // Connect to SQL
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    string query = "select top 1 pa_id from Partido where pa_estado = 'T' and pa_hora_ranking is null";

                    using (SqlCommand command3 = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command3.ExecuteReader())
                        {
                            int idPartido = 0;
                            string htmlCuerpoCorreo = "";


                            while (reader.Read())
                            {
                                idPartido = reader.GetInt32(0);

                                htmlCuerpoCorreo = ArmaHTMLRanking();

                                Console.WriteLine("Enviando correo de ranking");
                                EnvioCorreo("Ranking Quiniela " + DateTime.Now.ToString(), htmlCuerpoCorreo);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Ocurrio un error actualizando los partidos:" + e.ToString());
            }
        }

        private static string ArmaHTMLRanking()
        {
            string htmlPronosticos = "", query = "", trClass = "";
            int cont = 0, ranking = 0, puntosAnt = 0;

            string bg = ConfigurationManager.AppSettings["BackgroundColor"].ToString(), trPar = ConfigurationManager.AppSettings["trPar"].ToString(), trImpar = ConfigurationManager.AppSettings["trImpar"].ToString();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

            // Connect to SQL
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                query = "EXEC sp_tabla_ranking";

                using (SqlCommand cmRanking = new SqlCommand(query, connection))
                {
                    using(SqlDataReader rMarcadoresR = cmRanking.ExecuteReader()){
                        htmlPronosticos = "<table align=\"center\" cellpadding=\"2\" cellspacing=\"2\" border=\"0\">" +
                      "<tr style=\"font-family:Verdana;font-size:11px;font-weight: bold;padding:0px 10px 0px 0px;color:#FFFFFF;background-color:#" + bg + ";\">" +
                        "<td align=\"center\">No.</td>" +
                        "<td align=\"center\">Posici&oacute;n</td>" +
                        "<td align=\"center\">Alias</td>" +
                        "<td align=\"center\">Puntos</td>" +
                      "</tr>";

                        while (rMarcadoresR.Read())
                        {

                            if ((cont % 2) == 0)
                            {
                                trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#" + trPar + ";";
                            }
                            else
                            {
                                trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#" + trImpar + ";";
                            }

                            if (rMarcadoresR.GetInt32(1) != puntosAnt)
                            {
                                puntosAnt = rMarcadoresR.GetInt32(1);
                                ranking++;
                            }

                            if((cont == 0) && (ranking == 0)){
                                ranking++;
                            }

                            htmlPronosticos += "<tr style=\"" + trClass + "\">" +
                                                       "<td>" + (cont + 1) + "</td>" +
                                                       "<td align=\"center\">" + ranking + "</td>" +
                                                       "<td align=\"center\">" + rMarcadoresR.GetString(0) + "</td>" +
                                                       "<td align=\"center\">" + rMarcadoresR.GetInt32(1) + "</td>" +
                                                "</tr>";

                            cont++;
                        }

                        htmlPronosticos += "</table>";
                    }

                }

                query = "update Partido set pa_hora_ranking = GETDATE() where pa_estado = 'T' and pa_hora_ranking is null";

                using (SqlCommand cmActualiza = new SqlCommand(query, connection))
                {
                    cmActualiza.ExecuteNonQuery();
                }
            }

            htmlPronosticos += "<br /><br />Recuerda que puedes ingresar al portal haciendo clic <a href =\"" + ConfigurationManager.AppSettings["HOME"].ToString() + "\">aqu&iacute;</a>.";

            return htmlPronosticos;
        }

        private static void EnvioCorreo(string asunto, string cuerpoMensaje){
            SMTPClass mail = new SMTPClass();
            string resEnvioMail = "", query = "", mensaje = "", listaCorreos = "";
            bool firstTime = true;
            string c = ConfigurationManager.AppSettings["C"].ToString(), p = ConfigurationManager.AppSettings["P"].ToString(), s = ConfigurationManager.AppSettings["S"].ToString();
            int po = Int32.Parse(ConfigurationManager.AppSettings["PO"].ToString());

            mail.HOST = s;
            mail.PORT = po;

            mail.SMTP_USERNAME = c;
            mail.SMTP_PASSWORD = p;
            mail.ENABLESSL = true;

            mensaje = "<html>" +
                                "<head>" +
                                    "<title>Quiniela</title>" +
                                "</head>" +
                                "<body>" +
                                cuerpoMensaje +
                                "</body>" +
                       "</html>";

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

            // Connect to SQL
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString)){
                connection.Open();

                query = "select us_correoElectronico from Usuario, AliasUsuario where us_id = al_idUsuario and us_estado = 'V' and al_estado = 'V' group by us_correoElectronico";

                Console.WriteLine("Inicia el envio de correo... ");

                using (SqlCommand cmCorreos = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = cmCorreos.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if(firstTime){
                                listaCorreos = reader.GetString(0);
                                firstTime = false;
                            }else{
                                listaCorreos += "," + reader.GetString(0);
                            }

                        }
                    }
                }

                resEnvioMail = mail.SendMail("DevApps", c, c, "", listaCorreos, asunto, true, mensaje, "");

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(resEnvioMail);

                XmlNode errorNode = xmlDoc.DocumentElement.SelectSingleNode("/envio_correo/resultado");

                string errorCode = errorNode.Attributes["codigo"].Value;
                string errorMessage = errorNode.InnerText;

                if(errorCode == "1"){
                    Console.WriteLine("Envio de correo exitoso");
                    Console.WriteLine(" ");
                }else{
                    Console.WriteLine("Error en el envío de correo: " + errorMessage);
                    Console.WriteLine("Destinatarios: " + listaCorreos);
                    Console.WriteLine(" ");
                }
            }
        }
    }
}
