
namespace Folixa
{
    public partial class LoginPage : ContentPage
    {
        private Conexion conexion;
        private byte[] selectedPhotoBytes;

        public LoginPage()
        {
            InitializeComponent();
            conexion = new Conexion();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {

            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            bool isAuthenticated = await Task.Run(() => conexion.iniciarSesion(username, password));

            if (isAuthenticated)
            {
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                messageLabel.Text = "Usuario o contraseña incorrectos";
                messageLabel.IsVisible = true;
            }
        }

        private void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            loginSection.IsVisible = false;
            registerSection.IsVisible = true;
        }

        private void OnCancelRegisterButtonClicked(object sender, EventArgs e)
        {
            registerSection.IsVisible = false;
            loginSection.IsVisible = true;
        }

        private async void OnSelectPhotoButtonClicked(object sender, EventArgs e)
        {
            var result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                selectedPhoto.Source = ImageSource.FromStream(() => stream);
                selectedPhotoBytes = ConvertToBytes(stream);
            }
        }

        private async void OnRegisterUserButtonClicked(object sender, EventArgs e)
        {
            bool success = await conexion.RegistrarUsuarioAsync(newUsernameEntry.Text, newEmailEntry.Text, newPasswordEntry.Text, selectedPhotoBytes);
            if (success)
            {
                await DisplayAlert("Éxito", "Usuario registrado exitosamente", "OK");
                registerSection.IsVisible = false;
                loginSection.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el usuario", "OK");
            }
        }

        private byte[] ConvertToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
