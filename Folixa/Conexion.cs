using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using static QRCoder.PayloadGenerator;

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
                // Log the exception message
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
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
                    discoteca.idDiscoteca = resultado.GetFloat("id_discoteca").ToString();

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
                string query = "SELECT user, email, seguidos, seguidores, foto, perfil FROM usuarios WHERE user = @username";
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
                            Perfil = resultado.GetInt32("perfil"),
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
                string query = "INSERT INTO seguidores (user, user_seguido) VALUES (@usuarioActual, @usuarioASeguir)";
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

        // Contar seguidos (En la BBDD se representa como user (usuario que sigue a alguien) y user_seguido (Al que sigue ese usuario))
        public async Task<int> ContarSeguidosAsync(string usuario)
        {
            int contador = 0;
            try
            {
                await conexion.OpenAsync();
                string query = "SELECT COUNT(*) FROM seguidores WHERE user = @usuario";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                contador = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                // Manejar la excepción según sea necesario
                return -1;
            }
            finally
            {
                conexion.Close();
            }
            return contador;
        }

        // Contar seguidores
        public async Task<int> ContarSeguidoresAsync(string usuario)
        {
            int contador = 0;
            try
            {
                await conexion.OpenAsync();
                string query = "SELECT COUNT(*) FROM seguidores WHERE user_seguido = @usuario";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                contador = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                // Manejar la excepción según sea necesario
                return -1;
            }
            finally
            {
                conexion.Close();
            }
            return contador;
        }

        // Actualizar seguidos y seguidores
        public async Task<bool> ActualizarSeguidosSeguidoresAsync(string usuario, int seguidos, int seguidores)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "UPDATE usuarios SET seguidos = @seguidos, seguidores = @seguidores WHERE user = @usuario";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@seguidos", seguidos);
                cmd.Parameters.AddWithValue("@seguidores", seguidores);
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


        // Función para ver a quién sigues
        public async Task<List<Seguido>> VerSeguidosAsync(string usuarioActual)
        {
            List<Seguido> seguidos = new List<Seguido>();
            try
            {
                await conexion.OpenAsync();
                MySqlCommand consulta = new MySqlCommand("SELECT * FROM seguidores WHERE user = @usuarioActual", conexion);
                consulta.Parameters.AddWithValue("@usuarioActual", usuarioActual);
                MySqlDataReader resultado = consulta.ExecuteReader();
                while (resultado.Read())
                {
                    Seguido seguido = new Seguido();
                    seguido.User = resultado.GetString("user_seguido");

                    seguidos.Add(seguido);
                }
                conexion.Close();
            }
            catch (MySqlException e)
            {
                // Manejar la excepción según sea necesario
                Console.WriteLine($"Error al obtener seguidos: {e.Message}");
                return null;
            }
            return seguidos;
        }

        // Función para dejar de seguir a un usuario
        public async Task<bool> DejarDeSeguirAsync(string usuarioActual, string usuarioASeguir)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "DELETE FROM seguidores WHERE user = @usuarioActual AND user_seguido = @usuarioASeguir";
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

        public async Task ActualizarValoracionDiscotecaAsync(Discoteca discoteca)
        {
            string query = "UPDATE Discotecas SET valoracion = @valoracion WHERE nombre = @nombre";
            if (conexion.State != ConnectionState.Open)
            {
                await conexion.OpenAsync();
            }
            using (var command = new MySqlCommand(query, conexion))
            {
                command.Parameters.AddWithValue("@valoracion", discoteca.Valoracion);
                command.Parameters.AddWithValue("@nombre", discoteca.Nombre);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Función para obtener comentarios
        public async Task<List<Comentario>> ObtenerComentariosAsync(int id_discoteca)
        {
            List<Comentario> comentarios = new List<Comentario>();
            try
            {
                if (conexion.State != ConnectionState.Open)
                {
                    await conexion.OpenAsync();
                }
                MySqlCommand consulta = new MySqlCommand("SELECT * FROM comentarios WHERE id_discoteca = @id_discoteca", conexion);
                consulta.Parameters.AddWithValue("@id_discoteca", id_discoteca);
                MySqlDataReader resultado = (MySqlDataReader)await consulta.ExecuteReaderAsync();

                while (resultado.Read())
                {
                    Comentario comentario = new Comentario
                    {
                        User = resultado.GetString("user"),
                        ComentarioTexto = resultado.GetString("comentario")
                    };

                    comentarios.Add(comentario);
                }
                resultado.Close();
                conexion.Close();
            }
            catch (MySqlException e)
            {
                conexion.Close();
                return null;
            }
            return comentarios;
        }




        // Función para agregar un comentario
        public async Task<bool> AgregarComentarioAsync(string comentario, string user, int id_discoteca)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "INSERT INTO comentarios (user, id_discoteca, comentario) VALUES (@user, @id_discoteca, @comentario)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@comentario", comentario);
                cmd.Parameters.AddWithValue("@id_discoteca", id_discoteca);

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

        public async Task<List<Entrada>> ObtenerEntradasAsync(int id_discoteca)
        {
            List<Entrada> entradas = new List<Entrada>();
            try
            {
                if (conexion.State != ConnectionState.Open)
                {
                    await conexion.OpenAsync();
                }
                MySqlCommand consulta = new MySqlCommand("SELECT * FROM entradas WHERE id_discoteca = @id_discoteca", conexion);
                consulta.Parameters.AddWithValue("@id_discoteca", id_discoteca);
                MySqlDataReader resultado = (MySqlDataReader)await consulta.ExecuteReaderAsync();

                while (resultado.Read())
                {
                    Entrada entrada = new Entrada
                    {
                        IdEntrada = resultado.GetInt32("id_entrada"),
                        Precio = resultado.GetDecimal("precio"),
                        Copas = resultado.GetInt32("copas"),
                        Info = resultado.GetString("info"),
                        Fecha = resultado.GetDateTime("fecha"),
                        Cantidad = resultado.GetInt32("cantidad")
                    };

                    entradas.Add(entrada);
                }
                resultado.Close();
                conexion.Close();
            }
            catch (MySqlException e)
            {
                conexion.Close();
                return null;
            }
            return entradas;
        }

        public async Task<List<Mensaje>> ObtenerMensajesAsync(string remitente, string destinatario)
        {
            List<Mensaje> mensajes = new List<Mensaje>();
            try
            {
                await conexion.OpenAsync();
                string query = "SELECT * FROM mensajes WHERE (remitente = @remitente AND destinatario = @destinatario) OR (remitente = @destinatario AND destinatario = @remitente)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@remitente", remitente);
                cmd.Parameters.AddWithValue("@destinatario", destinatario);

                using (MySqlDataReader resultado = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (resultado.Read())
                    {
                        Mensaje mensaje = new Mensaje
                        {
                            Texto = resultado.GetString("texto"),
                            IsSentByCurrentUser = resultado.GetString("remitente") == remitente
                        };
                        mensajes.Add(mensaje);
                    }
                }
                conexion.Close();
            }
            catch (MySqlException e)
            {
                // Manejar la excepción según sea necesario
                conexion.Close();
                return null;
            }
            return mensajes;
        }

        public async Task<bool> GuardarMensajeAsync(string remitente, string destinatario, string texto)
        {
            try
            {
                await conexion.OpenAsync();
                string query = "INSERT INTO mensajes (remitente, destinatario, texto) VALUES (@remitente, @destinatario, @texto)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@remitente", remitente);
                cmd.Parameters.AddWithValue("@destinatario", destinatario);
                cmd.Parameters.AddWithValue("@texto", texto);

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

    // Clase Discoteca
    public class Discoteca
    {
        public string Nombre { get; set; }
        public string Valoracion { get; set; }
        public string Ubicacion { get; set; }
        public string Descripcion { get; set; }
        public string Horario { get; set; }
        public string idDiscoteca { get; set; }
        public byte[] Imagen { get; set; }
        public List<ImageSource> Estrellas { get; set; }
    }

    // Clase Seguido
    public class Seguido
    {
        public string User { get; set; }
    }

    // Clase Usuario
    public class Usuario
    {
        public string User { get; set; }
        public string Email { get; set; }
        public int Perfil { get; set; } // 0: Usuario, 1: Discoteca
        public int Seguidos { get; set; }
        public int Seguidores { get; set; }
        public byte[] Foto { get; set; }
    }

    // Clase Comentario
    public class Comentario
    {
        public string User { get; set; }
        public string ComentarioTexto { get; set; }
    }

    // Clase Entrada
    public class Entrada
    {
        public int IdEntrada { get; set; }
        public decimal Precio { get; set; }
        public int Copas { get; set; }
        public string Info { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }

}
