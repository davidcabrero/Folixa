using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;

namespace Folixa
{
    public static class GlobalSettings
    {
        public static string UsuarioIniciado { get; set; }
    }

    class Conexion
    {
        public MySqlConnection conexion;
        public Conexion()
        {
            conexion = new MySqlConnection("Server = 127.0.0.1; Database = folixa; Uid = root; Pwd =; Port = 3306");
        }

        // Función para iniciar sesión
        public bool iniciarSesion(string user, string password)
        {
            try
            {
                conexion.Open();
                string query = "SELECT password FROM usuarios WHERE user = @username";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@username", user);

                GlobalSettings.UsuarioIniciado = user;

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string hashedPassword = result.ToString();
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                return false;
            }
            catch (Exception ex)
            {
                // Manejar la excepción según sea necesario
                return false;
            }
            finally
            {
                conexion.Close();
            }
        }

        // Función para registrar usuario
        public async Task<bool> RegistrarUsuarioAsync(string user, string email, string password, byte[] foto)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "INSERT INTO usuarios (user, email, password, foto) VALUES (@username, @useremail, @userpassword, @userfoto)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@username", user);
                cmd.Parameters.AddWithValue("@useremail", email);
                cmd.Parameters.AddWithValue("@userpassword", BCrypt.Net.BCrypt.HashPassword(password));
                cmd.Parameters.AddWithValue("@userfoto", foto);

                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Manejar la excepción según sea necesario
                return false;
            }
            finally
            {
                conexion.Close();
            }
        }

        // Función para obtener las discotecas
        public async Task<List<Discoteca>> ObtenerDiscotecasAsync()
        {
            List<Discoteca> discotecas = new List<Discoteca>();
            try
            {
                await conexion.OpenAsync();
                MySqlCommand consulta = new MySqlCommand("SELECT * FROM discotecas", conexion);
                MySqlDataReader resultado = consulta.ExecuteReader();
                while (resultado.Read())
                {
                    Discoteca discoteca = new Discoteca();
                    discoteca.Nombre = resultado.GetString("nombre");
                    discoteca.Valoracion = resultado.GetFloat("valoracion").ToString();
                    discoteca.Ubicacion = resultado.GetString("ubicacion");
                    discoteca.Descripcion = resultado.GetString("descripcion");
                    discoteca.Horario = resultado.GetString("horario");
                    discoteca.Imagen = (byte[])resultado["imagen"];

                    discotecas.Add(discoteca);
                }
                conexion.Close();
            }
            catch (MySqlException e)
            {
                return null;
            }
            return discotecas;
        }

        public async Task<Usuario> ObtenerDatosUsuarioAsync(string username)
        {
            Usuario usuario = null;
            try
            {
                await conexion.OpenAsync();
                string query = "SELECT user, email, seguidos, seguidores, foto FROM usuarios WHERE user = @username";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@username", username);

                using (MySqlDataReader resultado = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (resultado.Read())
                    {
                        usuario = new Usuario
                        {
                            User = resultado.GetString("user"),
                            Email = resultado.GetString("email"),
                            Seguidos = resultado.GetInt32("seguidos"),
                            Seguidores = resultado.GetInt32("seguidores"),
                            Foto = (byte[])resultado["foto"]
                        };
                    }
                }
                conexion.Close();
            }
            catch (MySqlException e)
            {
                // Manejar la excepción según sea necesario
                return null;
            }
            return usuario;
        }

        public async Task<bool> SeguirUsuarioAsync(string usuarioActual, string usuarioASeguir)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "UPDATE usuarios SET seguidos = seguidos + 1 WHERE user = @usuarioActual; " +
                               "UPDATE usuarios SET seguidores = seguidores + 1 WHERE user = @usuarioASeguir;";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@usuarioActual", usuarioActual);
                cmd.Parameters.AddWithValue("@usuarioASeguir", usuarioASeguir);

                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Manejar la excepción según sea necesario
                return false;
            }
            finally
            {
                conexion.Close();
            }
        }

    }


    // Cambiar el tipo de la propiedad Imagen en la clase Discoteca de byte a byte[]
    public class Discoteca
    {
        public string Nombre { get; set; }
        public string Valoracion { get; set; }
        public string Ubicacion { get; set; }
        public string Descripcion { get; set; }
        public string Horario { get; set; }
        public byte[] Imagen { get; set; }
        public List<ImageSource> Estrellas { get; set; }
    }

    // Clase Usuario
    public class Usuario
    {
        public string User { get; set; }
        public string Email { get; set; }
        public int Seguidos { get; set; }
        public int Seguidores { get; set; }
        public byte[] Foto { get; set; }
    }
}
