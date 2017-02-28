using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using ScjnUtilities;

namespace AsistenciaLibreria
{
    public class AsistenciaModel
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["Base"].ConnectionString;

        public Usuarios GetCurrentUsuario(string username)
        {
            OleDbCommand cmd;
            OleDbDataReader reader;
            OleDbConnection connection = new OleDbConnection(connectionString);

            Usuarios usuario = null;

            try
            {
                connection.Open();

                string sSql = "SELECT P.IdPersonal, P.NombreCompleto, L.IdLibreria, L.Nombre " +
                              "FROM C_Personal P INNER JOIN C_Libreria L ON P.IdLiberia = L.IdLibreria " +
                              "WHERE Usuario = @Usuario";
                cmd = new OleDbCommand(sSql, connection);
                cmd.Parameters.AddWithValue("@Usuario", username);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    usuario = new Usuarios();
                    usuario.IdUsuario = Convert.ToInt32(reader["IdPersonal"]);
                    usuario.Usuario = reader["NombreCompleto"].ToString();
                    usuario.IdLibreria = Convert.ToInt32(reader["IdLibreria"]);
                    usuario.Libreria = reader["Nombre"].ToString();
                }
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AccesoModel", "BusquedaLatinos");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AccesoModel", "BusquedaLatinos");
            }
            finally
            {
                connection.Close();
            }

            return usuario;
        }

        /// <summary>
        /// Verifica si el usuario registro su entrada previamente
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public int DoUserCheckInToday(Usuarios usuario)
        {
            int doCheckIn = -3;
            OleDbCommand cmd;
            OleDbDataReader reader;
            OleDbConnection connection = new OleDbConnection(connectionString);

            try
            {
                connection.Open();

                string sSql = "SELECT * FROM Asistencia WHERE FechaInt = @Fecha AND IdPersonal = @Id";
                cmd = new OleDbCommand(sSql, connection);
                cmd.Parameters.AddWithValue("@Fecha", DateTimeUtilities.DateToInt(DateTime.Now));
                cmd.Parameters.AddWithValue("@Id", usuario.IdUsuario);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    doCheckIn = Convert.ToInt32(reader["IdAsistencia"]);
                }
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            finally
            {
                connection.Close();
            }

            return doCheckIn;
        }

        public bool SetCheckIn(Usuarios usuario)
        {
            bool checkInComplete = false;

            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter dataAdapter;

            DataSet dataSet = new DataSet();
            DataRow dr;

            try
            { 
                int id = DataBaseUtilities.GetNextIdForUse("Asistencia", "IdAsistencia", connection);

                if (id != 0)
                {
                    dataAdapter = new OleDbDataAdapter();
                    dataAdapter.SelectCommand = new OleDbCommand("SELECT * FROM Asistencia WHERE IdAsistencia = 0", connection);

                    dataAdapter.Fill(dataSet, "Asistencia");

                    dr = dataSet.Tables["Asistencia"].NewRow();
                    dr["IdAsistencia"] = id;
                    dr["IdPersonal"] = usuario.IdUsuario;
                    dr["IdLibreria"] = usuario.IdLibreria;
                    dr["Fecha"] = DateTime.Now;
                    dr["FechaInt"] = DateTimeUtilities.DateToInt(DateTime.Now);

                    dataSet.Tables["Asistencia"].Rows.Add(dr);

                    dataAdapter.InsertCommand = connection.CreateCommand();
                    dataAdapter.InsertCommand.CommandText = "INSERT INTO Asistencia(IdAsistencia,IdPersonal,IdLibreria,Fecha,HoraEntrada,FechaInt) " +
                                                            "VALUES (@IdAsistencia,@IdPersonal,@IdLibreria,@Fecha,TIME(),@FechaInt)";

                    dataAdapter.InsertCommand.Parameters.Add("@IdAsistencia", OleDbType.Numeric, 0, "IdAsistencia");
                    dataAdapter.InsertCommand.Parameters.Add("@IdPersonal", OleDbType.Numeric, 0, "IdPersonal");
                    dataAdapter.InsertCommand.Parameters.Add("@IdLibreria", OleDbType.Numeric, 0, "IdLibreria");
                    dataAdapter.InsertCommand.Parameters.Add("@Fecha", OleDbType.Date, 0, "Fecha");
                    dataAdapter.InsertCommand.Parameters.Add("@FechaInt", OleDbType.Numeric, 0, "FechaInt");

                    dataAdapter.Update(dataSet, "Asistencia");

                    dataSet.Dispose();
                    dataAdapter.Dispose();
                    connection.Close();
                    checkInComplete = true;
                }
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            finally
            {
                connection.Close();
            }

            return checkInComplete;
        }

        /// <summary>
        /// Actualiza la hora de entrada de un usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="idAsistencia"></param>
        /// <returns></returns>
        public bool UpdateCheckIn(Usuarios usuario, int idAsistencia)
        {
            bool updateComplete = false;

            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();

            DataSet dataSet = new DataSet();
            DataRow dr;
            try
            {
                string sqlCadena = "SELECT * FROM Asistencia WHERE IdAsistencia = @Asist";
                dataAdapter.SelectCommand = new OleDbCommand(sqlCadena, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Asist", idAsistencia);
                dataAdapter.Fill(dataSet, "Asistencia");

                dr = dataSet.Tables["Asistencia"].Rows[0];
                dr.BeginEdit();
                dr["IdAsistencia"] = idAsistencia;
                dr.EndEdit();

                dataAdapter.UpdateCommand = connection.CreateCommand();
                dataAdapter.UpdateCommand.CommandText =
                    "UPDATE Asistencia SET HoraEntrada = TIME() WHERE IdAsistencia = @IdAsistencia";

                dataAdapter.UpdateCommand.Parameters.Add("@IdAsistencia", OleDbType.Numeric, 0, "IdAsistencia");

                dataAdapter.Update(dataSet, "Asistencia");

                dataSet.Dispose();
                dataAdapter.Dispose();
                connection.Close();

                updateComplete = true;
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            finally
            {
                connection.Close();
            }
            return updateComplete;
        }


        public bool SetObservacionEntrada(Usuarios usuario, int idAsistencia, string observacion)
        {
            bool updateComplete = false;

            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();

            DataSet dataSet = new DataSet();
            DataRow dr;
            try
            {
                string sqlCadena = "SELECT * FROM Asistencia WHERE IdAsistencia = @Asist";
                dataAdapter.SelectCommand = new OleDbCommand(sqlCadena, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Asist", idAsistencia);
                dataAdapter.Fill(dataSet, "Asistencia");

                dr = dataSet.Tables["Asistencia"].Rows[0];
                dr.BeginEdit();
                dr["ObservacionesUser"] = observacion;
                dr.EndEdit();

                dataAdapter.UpdateCommand = connection.CreateCommand();
                dataAdapter.UpdateCommand.CommandText =
                    "UPDATE Asistencia SET ObservacionesUser = @ObservacionesUser WHERE IdAsistencia = @IdAsistencia";

                dataAdapter.UpdateCommand.Parameters.Add("@ObservacionesUser", OleDbType.VarChar, 0, "ObservacionesUser");
                dataAdapter.UpdateCommand.Parameters.Add("@IdAsistencia", OleDbType.Numeric, 0, "IdAsistencia");

                dataAdapter.Update(dataSet, "Asistencia");

                dataSet.Dispose();
                dataAdapter.Dispose();
                connection.Close();

                updateComplete = true;
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            finally
            {
                connection.Close();
            }
            return updateComplete;
        }


        public bool SetCheckOut(Usuarios usuario)
        {
            bool updateComplete = false;

            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();

            DataSet dataSet = new DataSet();
            DataRow dr;
            try
            {
                string sqlCadena = "SELECT * FROM Asistencia WHERE FechaInt = @Fecha AND IdPersonal = @IdPersonal";
                dataAdapter.SelectCommand = new OleDbCommand(sqlCadena, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Fecha", DateTimeUtilities.DateToInt(DateTime.Now));
                dataAdapter.SelectCommand.Parameters.AddWithValue("@IdPersonal", usuario.IdUsuario);
                dataAdapter.Fill(dataSet, "Asistencia");

                dr = dataSet.Tables["Asistencia"].Rows[0];
                dr.BeginEdit();
                dr["IdPersonal"] = usuario.IdUsuario;
                dr.EndEdit();

                dataAdapter.UpdateCommand = connection.CreateCommand();
                dataAdapter.UpdateCommand.CommandText =
                    "UPDATE Asistencia SET HoraSalida = TIME() WHERE FechaInt = @Fecha AND IdPersonal = @IdPersonal";

                dataAdapter.UpdateCommand.Parameters.Add("@Fecha", OleDbType.Numeric, 0, "FechaInt");
                dataAdapter.UpdateCommand.Parameters.Add("@IdPersonal", OleDbType.Numeric, 0, "IdPersonal");
                dataAdapter.Update(dataSet, "Asistencia");

                dataSet.Dispose();
                dataAdapter.Dispose();
                connection.Close();

                updateComplete = true;
            }
            catch (OleDbException ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                ErrorUtilities.SetNewErrorMessage(ex, methodName + " Exception,AsistenciaModel", "AsistenciaLibreria");
            }
            finally
            {
                connection.Close();
            }
            return updateComplete;
        }
    }
}