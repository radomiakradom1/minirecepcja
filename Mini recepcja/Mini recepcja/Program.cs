using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    // Klasa reprezentująca dane gościa  
    public class Guest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime VisitDate { get; set; } // Nowa właściwość na datę wizyty  

        public override string ToString()
        {
            return $"Imię: {Name}, Nazwisko: {Surname}, Email: {Email}, Telefon: {Phone}, Data wizyty: {VisitDate.ToShortDateString()}";
        }

        public string ToFileString()
        {
            return $"{Name};{Surname};{Email};{Phone};{VisitDate.ToString("yyyy-MM-dd")}";
        }

        public static Guest FromFileString(string fileString)
        {
            var parts = fileString.Split(';');
            return new Guest
            {
                Name = parts[0],
                Surname = parts[1],
                Email = parts[2],
                Phone = parts[3],
                VisitDate = DateTime.Parse(parts[4]) // Odczyt daty wizyty  
            };
        }
    }

    static void Main(string[] args)
    {
        List<Guest> guests = new List<Guest>();
        string filePath = "guests.txt"; // Bezpośrednia ścieżka do pliku w folderze projektu  

        // Ładowanie istniejących gości z pliku  
        LoadGuestsFromFile(filePath, guests);
        Console.WriteLine("Odczytane dane gości:");
        foreach (var g in guests)
        {
            Console.WriteLine(g);
        }

        // Wprowadzenie nowych gości  
        while (true)
        {
            Guest guest = new Guest();

            Console.Write("Podaj imię gościa: ");
            guest.Name = Console.ReadLine();

            Console.Write("Podaj nazwisko gościa: ");
            guest.Surname = Console.ReadLine();

            Console.Write("Podaj email gościa: ");
            guest.Email = Console.ReadLine();

            Console.Write("Podaj telefon gościa: ");
            guest.Phone = Console.ReadLine();

            Console.Write("Podaj datę wizyty (yyyy-MM-dd): ");
            guest.VisitDate = DateTime.Parse(Console.ReadLine()); // Wprowadzenie daty wizyty  

            guests.Add(guest);
            SaveGuestsToFile(guests, filePath); // Zapisz po dodaniu nowego gościa  

            Console.Write("Czy chcesz dodać kolejnego gościa? (t/n): ");
            string response = Console.ReadLine();
            if (response.ToLower() != "t")
                break;
        }

        // Zapytanie użytkownika, czy chce wysłać e-maile
        Console.WriteLine("Czy chcesz wysłać przypomnienia e-mail do gości, których wizyta jest jutro? (t/n): ");
        string sendEmailResponse = Console.ReadLine();

        if (sendEmailResponse.ToLower() == "t")
        {
            // Sprawdzenie dat wizyt i wysyłanie przypomnień  
            SendReminders(guests);
        }

        Console.WriteLine("Dziękujemy! Program zakończony.");
    }

    static void SaveGuestsToFile(List<Guest> guests, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var guest in guests)
            {
                writer.WriteLine(guest.ToFileString());
            }
        }
    }

    static void LoadGuestsFromFile(string filePath, List<Guest> guests)
    {
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        guests.Add(Guest.FromFileString(line));
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Plik nie istnieje, nie ma danych do załadowania.");
        }
    }

    static void SendReminders(List<Guest> guests)
    {
        DateTime today = DateTime.Today;
        DateTime tomorrow = today.AddDays(1);

        foreach (var guest in guests)
        {
            if (guest.VisitDate.Date == tomorrow)
            {
                // Wysyłamy przypomnienie na email  
                SendEmail(guest.Email, guest.Name, guest.Surname, guest.VisitDate);
            }
        }
    }

    static void SendEmail(string recipientEmail, string name, string surname, DateTime visitDate)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("cwlkonfident", "vintedfil@gmail.com"));
            message.To.Add(new MailboxAddress($"{name} {surname}", recipientEmail));
            message.Subject = "Przypomnienie o wizycie";

            var body = new TextPart("plain")
            {
                Text = $"Cześć {name},\n\nPrzypominamy o Twojej wizycie w dniu {visitDate.ToShortDateString()}.\n\nPozdrawiamy,\nZespół"
            };

            message.Body = body;

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("vintedfil@gmail.com", "cwlkonfident"); // Hasło aplikacji
                client.Send(message);
                client.Disconnect(true);
            }

            Console.WriteLine($"Przypomnienie wysłane do {name} {surname} na adres {recipientEmail}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas wysyłania e-maila: {ex.Message}");
        }
    }
}
    