using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;

namespace Folixa
{
    class Conexion
    {
        public MySqlConnection conexion;
        public Conexion()
        {
            conexion = new MySqlConnection("Server = 127.0.0.1; Database = folixa; Uid = root; Pwd =; Port = 3306");
        }

        //Para iniciar sesión
        public Boolean iniciarSesion(String user, String password)
        {
            try
            {
                conexion.Open();
                MySqlCommand consulta =
                    new MySqlCommand("SELECT * FROM usuario WHERE user = @user ", conexion); //Se verifica que el dni de la bbdd coincide con el introducido
                consulta.Parameters.AddWithValue("@user", user);
                MySqlDataReader resultado = consulta.ExecuteReader();

                if (resultado.Read())
                {
                    string passwordConHash = resultado.GetString("password"); //Se pasa la contraseña de la bbdd a string

                    if (BCrypt.Net.BCrypt.Verify(password, passwordConHash)) //Se verifica que la contraseña introducida y la encriptada son la misma
                    {
                        return true;
                    }

                }

                conexion.Close();
                return false;
            }
            catch (MySqlException e)
            {
                return false; //No accede
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
    }

    // Cambiar el tipo de la propiedad Imagen en la clase Discoteca de byte a byte[]
    public class Discoteca
    {
        public string Nombre { get; set; }
        public string Valoracion { get; set; }
        public string Ubicacion { get; set; }
        public string Descripcion { get; set; }
        public byte[] Imagen { get; set; }
        public List<ImageSource> Estrellas { get; set; }
    }
}