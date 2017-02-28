using System;
using System.Linq;
using System.Windows;
using ScjnUtilities;

namespace AsistenciaLibreria
{
    /// <summary>
    /// Interaction logic for Retardo.xaml
    /// </summary>
    public partial class Retardo : Window
    {
        Usuarios usuario;

        public Retardo(Usuarios usuario)
        {
            InitializeComponent();
            this.usuario = usuario;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            TxtObservacion.Text = VerificationUtilities.TextBoxStringValidation(TxtObservacion.Text);

            if (TxtObservacion.Text.Length < 10)
            {
                MessageBox.Show("Debes ingresar alguna observación de lo contrario oprime el botón cancelar");
                return;
            }

            AsistenciaModel model = new AsistenciaModel();

            int idAsistencia = model.DoUserCheckInToday(usuario);

            bool complete = model.SetObservacionEntrada(usuario, idAsistencia, TxtObservacion.Text);

            if (complete)
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("No se pudo completar el registro de las observaciones, favor de volverlo a intentar");
            }


        }
    }
}
