using System;
using System.Linq;
using System.Windows;

namespace AsistenciaLibreria
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string username;

        private Usuarios usuario;


        TimeSpan tolerancia = new TimeSpan(9, 16, 0);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            username = Environment.UserName;

            usuario = new AsistenciaModel().GetCurrentUsuario(username);

            if (usuario != null)
            {
                LblLibreria.Content = usuario.Libreria;
                LblUsuario.Content = usuario.Usuario;
            }
            else
            {
                MessageBox.Show("No estas registrado en el sistema, favor de comunicarte con tu superior jerárquico");
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime hora = DateTime.Now;

            AsistenciaModel model = new AsistenciaModel();

            int idAsistencia = model.DoUserCheckInToday(usuario);

            if (idAsistencia != -3)
            {
                MessageBoxResult result = MessageBox.Show("Ya habías registrado la hora de entrada del día de hoy. ¿Deseas sustituirla por la hora actual?", "Atención", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    model.UpdateCheckIn(usuario, idAsistencia);
                }
            }
            else
            {
                bool chekInComplete = model.SetCheckIn(usuario);

                if (!chekInComplete)
                {
                    MessageBox.Show("No se pudo completar el registro, favor de volver a intentarlo");
                    return;
                }

                

            }

            if (hora.TimeOfDay > tolerancia)
            {
                Retardo retardo = new Retardo(usuario) { Owner = this };
                retardo.ShowDialog();
            }

            MessageBox.Show(String.Format("Hora de entrada registrada: {0}", hora));
            this.Close();
        }

        private void BtnSalida_Click(object sender, RoutedEventArgs e)
        {
            AsistenciaModel model = new AsistenciaModel();

            int idAsistencia = model.DoUserCheckInToday(usuario);

            if (idAsistencia == -3) {
                MessageBox.Show("Para poder registrar la salida primero debes de registrar la entrada");
                return;
            }
            
            model.SetCheckOut(usuario);




            MessageBox.Show(String.Format("Hora de salida registrada: {0}", DateTime.Now));
            this.Close();
        }
    }
}
