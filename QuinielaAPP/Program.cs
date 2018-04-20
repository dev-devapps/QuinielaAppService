// Program.cs
using System;
using System.Configuration;
using System.Data.SqlClient;


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


            Console.ReadKey(true);
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
                    }

                    query = "select pa_id, E1.eq_descripcion 'Equipo1', E2.eq_descripcion 'Equipo1' from Partido, Equipo as E1, Equipo as E2 where pa_estado = 'C' and E1.eq_id = pa_idEquipo1 and E2.eq_id = pa_idEquipo2 and pa_hora_pronostico is null";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int idPartido = 0;
                            string equipo1 = "", equipo2 = "", htmlCuerpoCorreo = "";


                            while (reader.Read())
                            {
                                idPartido = reader.GetInt32(0);
                                equipo1 = reader.GetString(1);
                                equipo2 = reader.GetString(2);

                                htmlCuerpoCorreo = ArmaHTMLPronostico(idPartido, equipo1, equipo2);

                                //EnvioCorreo("Pronosticos " + equipo1 + " vs. " + equipo2, htmlCuerpoCorreo);
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

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

            // Connect to SQL
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                query = "EXEC sp_tabla_pronostico @idPartido";


                using (SqlCommand cmMarcador = new SqlCommand(query, connection))
                {

                    cmMarcador.Parameters.AddWithValue("@idPartido", idPartido);
                    SqlDataReader rMarcadores = cmMarcador.ExecuteReader();

                    htmlPronosticos = "<table align=\"center\" cellpadding=\"2\" cellspacing=\"2\" border=\"0\">" +
                      "<tr style=\"font-family:Verdana;font-size:11px;font-weight: bold;padding:0px 10px 0px 0px;color:#FFFFFF;background-color:#0B0B3B;\">" +
                        "<td align=\"center\">Alias</td>" +
                        "<td align=\"center\">" + equipo1 + "</td>" +
                        "<td align=\"center\">" + equipo2 + "</td>" +
                        "<td align=\"center\">Fecha</td>" +
                      "</tr>";

                    while (rMarcadores.Read())
                    {

                        if ((cont % 2) == 0)
                        {
                            trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#1E5E9E;";
                        }
                        else
                        {
                            trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#58ACFA;";
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

                query = "update Partido set pa_hora_pronostico = GETDATE() where pa_id = @idPartido";

                using(SqlCommand cmActualiza = new SqlCommand(query, connection)){
                    cmActualiza.Parameters.AddWithValue("@idPartido", idPartido);

                    cmActualiza.ExecuteNonQuery();
                }
            }



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

                    string query = "select pa_id from Partido where pa_estado = 'T' and pa_hora_ranking is null";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int idPartido = 0;
                            string htmlCuerpoCorreo = "";


                            while (reader.Read())
                            {
                                idPartido = reader.GetInt32(0);

                                htmlCuerpoCorreo = ArmaHTMLRanking();

                                //EnvioCorreo("Ranking Quiniela " + DateTime.Now.ToString(), htmlCuerpoCorreo);
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
            int cont = 0, ranking = 1, puntosAnt = 0;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings[SQLCONNSTRING].ConnectionString;

            // Connect to SQL
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                query = "EXEC sp_tabla_ranking";

                using (SqlCommand cmMarcador = new SqlCommand(query, connection))
                {
                    SqlDataReader rMarcadores = cmMarcador.ExecuteReader();

                    htmlPronosticos = "<table align=\"center\" cellpadding=\"2\" cellspacing=\"2\" border=\"0\">" +
                      "<tr style=\"font-family:Verdana;font-size:11px;font-weight: bold;padding:0px 10px 0px 0px;color:#FFFFFF;background-color:#0B0B3B;\">" +
                        "<td align=\"center\">No.</td>" +
                        "<td align=\"center\">Posici&oacute;n</td>" +
                        "<td align=\"center\">Alias</td>" +
                        "<td align=\"center\">Puntos</td>" +
                      "</tr>";

                    while (rMarcadores.Read())
                    {

                        if ((cont % 2) == 0)
                        {
                            trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#1E5E9E;";
                        }
                        else
                        {
                            trClass = "font-family:Verdana;font-size:11px;padding:0px 5px 0px 0px;color:#FFFFFF;background-color:#58ACFA;";
                        }

                        if(rMarcadores.GetInt32(1) == puntosAnt){
                            puntosAnt = rMarcadores.GetInt32(1);
                            ranking++;
                        }

                        htmlPronosticos += "<tr style=\"" + trClass + "\">" +
                                                   "<td>" + (cont + 1) + "</td>" +
                                                   "<td align=\"center\">" + ranking + "</td>" +
                                                   "<td align=\"center\">" + rMarcadores.GetString(0) + "</td>" +
                                                   "<td align=\"center\">" + rMarcadores.GetInt32(1) + "</td>" +
                                            "</tr>";

                        cont++;
                    }

                    htmlPronosticos += "</table>";
                }

                query = "update Partido set pa_hora_ranking = GETDATE() where pa_estado = 'T' and pa_hora_ranking is null";

                using (SqlCommand cmActualiza = new SqlCommand(query, connection))
                {
                    cmActualiza.ExecuteNonQuery();
                }
            }



            return htmlPronosticos;
        }

        private static void EnvioCorreo(string asunto, string cuerpoMensaje){
            SMTPClass mail = new SMTPClass();
            string resEnvioMail = "", query = "", correo = "", mensaje = "";

            mail.HOST = "";
            mail.PORT = 25;

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

                query = "select us_correoElectronico from Usuario";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            correo = reader.GetString(0);

                            resEnvioMail = mail.SendMail("Quiniela Rusia 2018", "info@devapps.com", correo, "", "", asunto, true, mensaje, "");
                        }
                    }
                }
            }
        }
    }

    /*class Pronostico{
        public string alias { get; set; }

        public int id_alias { get; set; }

        public int marcador1 { get; set; }

        public int marcador2 { get; set; }

        public int pa_marcador1 { get; set; }

        public int pa_marcador2 { get; set; }

        public string puntos { get; set; }

        public string hora { get; set; }

        public int CalculaPuntos(){
            
        }
    }*/
}
