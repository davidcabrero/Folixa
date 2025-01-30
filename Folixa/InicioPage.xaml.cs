using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PaypalServerSdk;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Maui.ApplicationModel;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using System.Drawing;
using System.Drawing.Imaging;

namespace Folixa
{
    public partial class InicioPage : ContentPage
    {
        private Conexion conexion;
        private Entrada entradaSeleccionada;
        public List<Discoteca> discotecas;
        public List<Discoteca> discotecasFiltradas;
        public List<Comentario> comentarios;
        public List<string> Estrellas { get; set; } = new List<string>();
        public ICommand DiscotecaSelectedCommand { get; }
        public ICommand EntradaSelectedCommand { get; }

        public InicioPage()
        {
            InitializeComponent();
            conexion = new Conexion();
            CargarDiscotecas();
            searchBar.TextChanged += buscarDiscoteca;
            DiscotecaSelectedCommand = new Command<Discoteca>(OnDiscotecaSelected);
            EntradaSelectedCommand = new Command<Entrada>(OnEntradaSelected);
            BindingContext = this;
        }

        private async void CargarDiscotecas()
        {
            discotecas = await conexion.ObtenerDiscotecasAsync();
            discotecasFiltradas = new List<Discoteca>(discotecas);
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }

        private void buscarDiscoteca(object sender, TextChangedEventArgs e)
        {
            string textoBusqueda = searchBar.Text.ToLower();
            discotecasFiltradas = discotecas.Where(d => d.Nombre.ToLower().Contains(textoBusqueda)).ToList();
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }

        private void OnDiscotecaSelected(Discoteca selectedDiscoteca)
        {
            if (selectedDiscoteca != null)
            {
                // Actualizar la UI con la información de la discoteca seleccionada
                nombreDiscoteca.Text = selectedDiscoteca.Nombre;
                imagenDiscoteca.Source = ImageSource.FromStream(() => new MemoryStream(selectedDiscoteca.Imagen));
                ubicacionDiscoteca.Text = selectedDiscoteca.Ubicacion;
                descripcionDiscoteca.Text = selectedDiscoteca.Descripcion;

                // Generar las estrellas según la valoración
                selectedDiscoteca.Estrellas = GenerarEstrellas(selectedDiscoteca.Valoracion);

                // Asignar las estrellas a la vista de detalles
                estrellasDiscoteca.Children.Clear();
                foreach (var estrella in selectedDiscoteca.Estrellas)
                {
                    estrellasDiscoteca.Children.Add(new Microsoft.Maui.Controls.Image { Source = estrella });
                }

                // Mostrar la sección de información de la discoteca
                busquedaDiscotecasSection.IsVisible = false;
                DiscotecaSection.IsVisible = true;

                // Asignar el valor del picker a la valoración actual
                valoracionPicker.SelectedIndex = int.Parse(selectedDiscoteca.Valoracion);
            }
        }

        private List<ImageSource> GenerarEstrellas(string valoracion)
        {
            int valoracionNumerica = int.Parse(valoracion);
            var estrellas = new List<ImageSource>();
            for (int i = 0; i < 5; i++)
            {
                if (i < valoracionNumerica)
                {
                    estrellas.Add(ImageSource.FromFile("star_filled.png"));
                }
                else
                {
                    estrellas.Add(ImageSource.FromFile("star_empty.png"));
                }
            }
            return estrellas;
        }

        private async void OnValoracionPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiscotecaSection.IsVisible)
            {
                var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);
                if (selectedDiscoteca != null)
                {
                    // Calcular la nueva valoración como la media entre la valoración existente y la nueva
                    int valoracionExistente = int.Parse(selectedDiscoteca.Valoracion);
                    int nuevaValoracion = (valoracionExistente + valoracionPicker.SelectedIndex) / 2;
                    selectedDiscoteca.Valoracion = nuevaValoracion.ToString();
                    selectedDiscoteca.Estrellas = GenerarEstrellas(selectedDiscoteca.Valoracion);

                    // Actualizar las estrellas en la UI
                    estrellasDiscoteca.Children.Clear();
                    foreach (var estrella in selectedDiscoteca.Estrellas)
                    {
                        estrellasDiscoteca.Children.Add(new Microsoft.Maui.Controls.Image { Source = estrella });
                    }

                    // Actualizar la valoración en la base de datos
                    await conexion.ActualizarValoracionDiscotecaAsync(selectedDiscoteca);
                }
            }
        }

        private async void OnVerComentariosButtonClicked(object sender, EventArgs e)
        {
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);

            if (selectedDiscoteca != null)
            {
                comentarios = await conexion.ObtenerComentariosAsync(idDiscotecaInt);
                comentariosCollectionView.ItemsSource = comentarios;

                // Mostrar la sección de comentarios
                DiscotecaSection.IsVisible = false;
                ComentariosSection.IsVisible = true;
            }
        }

        private async void OnEnviarComentarioButtonClicked(object sender, EventArgs e)
        {

            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);


            bool success = await conexion.AgregarComentarioAsync(nuevoComentarioEntry.Text, GlobalSettings.UsuarioIniciado, idDiscotecaInt);
            if (success)
            {
                await DisplayAlert("Éxito", "Comentario subido", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el comentario", "OK");
            }
        }

        private void OnVolverComentariosButtonClicked(object sender, EventArgs e)
        {
            ComentariosSection.IsVisible = false;
            DiscotecaSection.IsVisible = true;
        }

        private void OnVolverButtonClicked(object sender, EventArgs e)
        {
            busquedaDiscotecasSection.IsVisible = true;
            DiscotecaSection.IsVisible = false;
        }

        public List<Entrada> entradas;

        private async void OnVerEntradasButtonClicked(object sender, EventArgs e)
        {
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);

            if (selectedDiscoteca != null)
            {
                entradas = await conexion.ObtenerEntradasAsync(idDiscotecaInt);
                entradasCollectionView.ItemsSource = entradas;

                // Mostrar la sección de entradas
                DiscotecaSection.IsVisible = false;
                verEntradasSection.IsVisible = true;
            }
        }

        private void OnVolverEntradasButtonClicked(object sender, EventArgs e)
        {
            verEntradasSection.IsVisible = false;
            DiscotecaSection.IsVisible = true;
        }

        private async void OnPagarConPayPalButtonClicked(object sender, EventArgs e)
        {
            // Implementar la lógica de pago con PayPal
            var result = await ProcesarPagoConPayPal();
            if (result)
            {
                // Generar el PDF con la entrada
                string pdfFilename = GenerarPDFEntrada(entradaSeleccionada);

                // Enviar el PDF por correo electrónico
                string emailUsuario = GlobalSettings.EmailIniciado;
                if (!string.IsNullOrEmpty(emailUsuario))
                {
                    await EnviarCorreoConEntradaAsync(emailUsuario, pdfFilename);
                }

                await DisplayAlert("Éxito", "Pago realizado y entrada descargada en documentos", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo realizar el pago", "OK");
            }
        }

        private async Task<bool> ProcesarPagoConPayPal()
        {
            // Implementar la lógica de integración con PayPal
            // Devuelve true si el pago se realizó con éxito, de lo contrario, false
            return true;
        }

        private string GenerarPDFEntrada(Entrada entrada)
        {
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);
            PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
            PdfSharpCore.Pdf.PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
            XFont textFont = new XFont("Arial", 16, XFontStyle.Regular);
            XBrush textColor = XBrushes.Black;

            double margin = 40;
            double qrCodeSize = 150;
            double textWidth = page.Width - qrCodeSize - 3 * margin;

            gfx.DrawString(selectedDiscoteca.Nombre, titleFont, textColor, new XRect(margin, margin, textWidth, 40), XStringFormats.TopLeft);
            gfx.DrawString("Fecha: " + entrada.Fecha.ToString("dd/MM/yyyy"), textFont, textColor, new XRect(margin, margin + 50, textWidth, 30), XStringFormats.TopLeft);
            gfx.DrawString("Precio: " + entrada.Precio.ToString("C"), textFont, textColor, new XRect(margin, margin + 90, textWidth, 30), XStringFormats.TopLeft);
            gfx.DrawString("Copas: " + entrada.Copas, textFont, textColor, new XRect(margin, margin + 130, textWidth, 30), XStringFormats.TopLeft);

            string qrCodeText = $"Entrada para {selectedDiscoteca.Nombre}\nFecha: {entrada.Fecha:dd/MM/yyyy}\nPrecio: {entrada.Precio:C}\nCopas: {entrada.Copas}";
            byte[] qrImage = GenerarCodigoQR(qrCodeText, (int)qrCodeSize);

            using (var ms = new MemoryStream(qrImage))
            {
                XImage xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));
                gfx.DrawImage(xImage, page.Width - qrCodeSize - margin, margin, qrCodeSize, qrCodeSize);
            }

            string pdfFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Entrada.pdf");
            document.Save(pdfFilename);
            return pdfFilename;
        }

        private byte[] GenerarCodigoQR(string texto, int size)
        {
            var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(texto, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }


        private async Task EnviarCorreoConEntradaAsync(string email, string pdfFilename)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Folixa", "entradasfolixa@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Tu entrada para la discoteca";

            var body = new TextPart("plain")
            {
                Text = "Adjunto encontrarás tu entrada para la discoteca."
            };

            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(File.OpenRead(pdfFilename), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(pdfFilename)
            };

            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);

            message.Body = multipart;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("entradasfolixa@gmail.com", "folixaCorreoEntradas#382");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        private void OnVolverComprarEntradasButtonClicked(object sender, EventArgs e)
        {
            comprarEntradasSection.IsVisible = false;
            verEntradasSection.IsVisible = true;
        }

        private void OnEntradaSelected(Entrada selectedEntrada)
        {
            if (selectedEntrada != null)
            {
                entradaSeleccionada = selectedEntrada;
                entradaInfoLabel.Text = $"Entrada para {selectedEntrada.Info} - Precio: {selectedEntrada.Precio:C}";
                verEntradasSection.IsVisible = false;
                comprarEntradasSection.IsVisible = true;
            }
        }

    }
}
