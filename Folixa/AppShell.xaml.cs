namespace Folixa
{
    public partial class AppShell : Shell
    {
        private Conexion conexion = new Conexion();
        private string username;

        public AppShell()
        {
            InitializeComponent();
            ConfigureAdminTabVisibilityAsync();
        }

        private async Task ConfigureAdminTabVisibilityAsync()
        {
            string username = GlobalSettings.UsuarioIniciado;

            var adminTab = this.FindByName<Tab>("AdminTab");
            Usuario usuario = await conexion.ObtenerDatosUsuarioAsync(username);
            if (usuario.Perfil == 2)
            {
                adminTab.IsVisible = true;
            }
        }
    }
}
