using System;
using System.Linq;

namespace AsistenciaLibreria
{
    public class Usuarios
    {
        private int idUsuario;
        private string usuario;
        private int idLibreria;
        private string libreria;

        public int IdUsuario
        {
            get
            {
                return this.idUsuario;
            }
            set
            {
                this.idUsuario = value;
            }
        }

        public string Usuario
        {
            get
            {
                return this.usuario;
            }
            set
            {
                this.usuario = value;
            }
        }

        public int IdLibreria
        {
            get
            {
                return this.idLibreria;
            }
            set
            {
                this.idLibreria = value;
            }
        }

        public string Libreria
        {
            get
            {
                return this.libreria;
            }
            set
            {
                this.libreria = value;
            }
        }
    }
}
