using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIPy
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            listBox1.Items.Add("Jak połączyć się z serwerem?");
            listBox1.Items.Add("Jak zmienić ustawienia dźwięku?");
            listBox1.Items.Add("Jak dodać serwer do ulubionych?");
            listBox1.Items.Add("Dodatkowe funkcjonalności");
            listBox1.SetSelected(0, true);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.GetSelected(0))
            {
                textBox2.Text = "Aby połączyć się z serwerem należy wpisać IP serwera w miejsce starego adresu (domyślnie 127.0.01) " + 
                    "oraz port serwera (jeśli nie jesteś pewny jaki jest port pozostaw domyślny 15000)." + Environment.NewLine + Environment.NewLine +
                    "Przed połączeniem z serwerem pamiętaj, aby uzupełnić swój pseudonim. Nie można się połączyć bez wcześniejszego ustawienia swojego \"nicku\". " +
                    "Aby tego dokonać wypełnij pole domyślnie uzupełnione jako \"Nickname...\". Po poprawnym uzupełnieniu adresu serwera oraz pseudonimu " +
                    "wciśnij przycisk \"POŁĄCZ\". Jeśli serwer istnieje zostaniesz pomyślnie podłączony. Jeśli wystąpią problemy nastąpi próba ponownego połączenia.";
            }
            else if (listBox1.GetSelected(1))
            {
                textBox2.Text = "Aby zmienić ustawienia dźwięku wybierz zakładkę \"Ustawienia\", a następnie wybierz \"Opcje\". Otworzy Ci się nowe okienko. " +
                    "W środku masz 3 ustawienia, które możesz zmienić." + Environment.NewLine + Environment.NewLine +
                    "Domyślny mikrofon: Urządzenia audio, z którego będzie nagrywany dźwięk;" + Environment.NewLine + Environment.NewLine +
                    "Bitrate: Ilość danych transmitowana na sekundę;" + Environment.NewLine + Environment.NewLine + 
                    "Bit Depth: Liczba bitów audio zawierająca się w każdej próbce.";
            }
            else if (listBox1.GetSelected(2))
            {
                textBox2.Text = "Aby dodać serwer do ulubionych wybierz zakładkę \"Połączenia\", a następnie wybierz \"Ulubione\". " +
                    "W polu tekstowym opisanym jako \"Adres serwera\" wpisz IP serwera wraz z jego portem w postaci IP:Port. " +
                    "Jeśli nie jesteś pewny portu wpisz 15000. Następnie w polu tekstowym opisanym jako \"Nazwa serwera\" " +
                    "wpisz dowolną nazwę jaką chcesz nadać serwerowi. Po wypełnieniu obu pól wciśnij przycisk \"DODAJ\". " +
                    "Serwer pojawił się na liście." + Environment.NewLine + Environment.NewLine +
                    "Aby usunąć serwer z listy ulubionych wystarczy zaznaczyć go na liście, a następnie wcisnąć " +
                    "przycisk \"USUŃ\"." + Environment.NewLine + Environment.NewLine +
                    "Aby się połączyć z ulubionym serwerem wystarczy zaznaczyć go na liście, a następnie wcisnąć " +
                    "przycisk \"POŁĄCZ\".";
            }
            else if (listBox1.GetSelected(3))
            {
                textBox2.Text = "Aby wyciszyć dźwięk wciśnij ikonę głośnika znajdującą się obok pola tekstowego z Twoim pseudonimem. " +
                    "Z wyciszonym dźwiękiem nie będziesz słyszał innych użytkowników serwera. Aby przywrócić dźwięk kliknij ponownie w tą samą ikonę." +
                    Environment.NewLine + Environment.NewLine +
                    "Aby wyciszyć mikrofon wciśnij ikonę mikrofonu znajdującą się obok pola tekstowego z Twoim pseudonimem. " +
                    "Z wyciszonym mikrofonem żaden z pozostałych użytkowników serwera nie będzie otrzymywał od Ciebie wiadomości audio.";
            }
        }
    }
}
