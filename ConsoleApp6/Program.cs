using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace ConsoleApp8
{
    class Generate
    {
        public static string GenerateNumber(int length)
        {
            Random random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }
    }

    public enum Category
    {
        IT = 0,
        HR = 1,
        DOCTOR = 2,
        JOURNALIST = 3,
        TRANSLATOR = 4
    };

    public enum Status
    {
        Worker = 0,
        Employer = 1
    };
    public enum Gender
    {
        Men = 0,
        Woman = 1
    };

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            General general = new General();

            try
            {
                int ext = 0;
                do
                {
                    Console.WriteLine("1.Sign In");
                    Console.WriteLine("2.Sign Up");
                    Console.WriteLine("0.Exit");
                    int.TryParse(Console.ReadLine(), out ext);
                    switch (ext)
                    {
                        case 1:
                            Console.Clear();
                            general.LogIn();
                            break;
                        case 2:
                            Console.Clear();
                            general.Reg();
                            break;
                        default:
                            break;
                    }
                } while (ext == 1 || ext == 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    [XmlInclude(typeof(Person))]
    public abstract class Person
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Status Status { get; set; }
        public string Phone { get; set; }
    }

    public class Worker : Person
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public Gender Gender { get; set; }
        public string Age { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public Category Category { get; set; }
        public string City { get; set; }
        public string Salary { get; set; }
        public Status Status { get; set; } = Status.Worker;
    }

    public class Employer : Person
    {
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public Category Category { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Salary { get; set; }
        public string Age { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public List<Worker> Worker { get; set; } = new List<Worker>();
        public Status Status { get; set; } = Status.Employer;
    }

    class General
    {
        JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
        List<Person> persons = new List<Person>();

        public void LogIn()
        {
            using (StreamReader red = new StreamReader(@"file.json"))
            {
                var new_persons = JsonConvert.DeserializeObject<List<Person>>(red.ReadToEnd(), settings);
                persons = new_persons;
            }
            var elanler = persons.OfType<Employer>().Cast<Employer>().ToList();
            int choose = 0;
            do
            {
                Console.WriteLine("Worker (1):");
                Console.WriteLine("Employer (2):");
                int.TryParse(Console.ReadLine(), out choose);
                Console.WriteLine("Username:");
                string username = Console.ReadLine();
                Console.WriteLine("Password:");
                string pass = Console.ReadLine();
                switch (choose)
                {
                    case 1:

                        Console.Clear();

                        bool yoxladiq = false;
                        do
                        {
                            var newList = persons.OfType<Worker>().Cast<Worker>().ToList();
                            var query = (from e in persons
                                         join a in newList on e.Status equals a.Status
                                         where a.Username == username && a.Password == pass && a.Status == Status.Worker
                                         select a).ToList();
                            int yoxla = 0;
                            for (int i = 0; i < query.Count(); i++)
                            {
                                var indexs = persons.IndexOf(query[i]);
                                Console.WriteLine($"Hello {query[i].Username}");
                                Console.WriteLine("1. Add CV or change CV");
                                Console.WriteLine("2. Search job (see the more relevant announcements)");
                                Console.WriteLine("3. Search job (choose your own)");
                                Console.WriteLine("4. Show your CV");
                                Console.WriteLine("5. See all job");
                                Console.WriteLine("0. Exit");
                                int.TryParse(Console.ReadLine(), out yoxla);
                                do
                                {
                                    switch (yoxla)
                                    {
                                        case 0:
                                            Console.Clear();
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        case 1:
                                            Console.Clear();
                                            var change = ChangeCV(query[i]);

                                            persons.Add(change);
                                            persons.RemoveAt(indexs);

                                            var jsons = JsonConvert.SerializeObject(persons, settings);

                                            using (StreamWriter writer = new StreamWriter("file.json"))
                                            {
                                                writer.WriteLine(jsons);
                                            }
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        case 2:
                                            Console.Clear();
                                            int say = 0;
                                            var secilielan = from e in elanler
                                                             where e.Status == Status.Employer && e.Salary == query[i].Salary && e.Category == query[i].Category && e.Experience == query[i].Experience
                                                             select e;
                                            foreach (var el in secilielan)
                                            {
                                                if (el.JobTitle != null)
                                                {
                                                    Console.WriteLine($"{++say}) Category : {el.Category} - Position name : {el.JobTitle} - Salary : {el.Salary} ");
                                                    Console.WriteLine("---------------------------------------------");
                                                }
                                                else { Console.WriteLine("Belə bir elan yoxdur"); }
                                            }
                                            if (say != 0)
                                            {
                                                say = 0;
                                                Console.WriteLine("Please enter the number of an announcement");
                                                int.TryParse(Console.ReadLine(), out int y);
                                                Console.Clear();
                                                foreach (var el in secilielan)
                                                {
                                                    if (el.JobTitle != null)
                                                    {
                                                        ++say;
                                                        if (say == y)
                                                        {
                                                            Console.WriteLine(
                                                            $" Category : {el.Category}  " +
                                                            Environment.NewLine +
                                                                $"Position name : {el.JobTitle}  " +
                                                            Environment.NewLine +
                                                                $"Company Name : {el.CompanyName}  " +
                                                            Environment.NewLine +
                                                                $"Job description : {el.Description}  " +
                                                            Environment.NewLine +
                                                                $"Region : {el.City}  " +
                                                            Environment.NewLine +
                                                                $"Age : {el.Age}  " +
                                                            Environment.NewLine +
                                                                $"Education : {el.Education}  " +
                                                            Environment.NewLine +
                                                                $"Experience : {el.Experience}  " +
                                                            Environment.NewLine +
                                                                $"Phone number: {el.Phone}  " +
                                                            Environment.NewLine +
                                                                $"Salary : {el.Salary} "
                                                                );

                                                            Console.WriteLine("Want to apply? (y/n)");
                                                            string muraciet = Console.ReadLine();
                                                            if (muraciet == "y" || muraciet == "Y")
                                                            {
                                                                AddMuraciet(el, query[i], persons.IndexOf(el));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else { Console.WriteLine("Announcement not found"); }
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        case 3:
                                            Console.Clear();
                                            int count = 0;
                                            Console.WriteLine("What do you want to search for? (Category (0) , " +
                                                "Education (1), Region (2), Salary (3), " +
                                                "Experience (4))");
                                            int.TryParse(Console.ReadLine(), out int x);
                                            IEnumerable<Employer> secilielan1 = null;
                                            switch (x)
                                            {
                                                case 0:
                                                    secilielan1 = from e in elanler
                                                                  where e.Status == Status.Employer && e.Category == query[i].Category
                                                                  select e;
                                                    break;
                                                case 1:
                                                    secilielan1 = from e in elanler
                                                                  where e.Status == Status.Employer && e.Education == query[i].Education
                                                                  select e;
                                                    break;
                                                case 2:
                                                    secilielan1 = from e in elanler
                                                                  where e.Status == Status.Employer && e.City == query[i].City
                                                                  select e;
                                                    break;
                                                case 3:
                                                    secilielan1 = from e in elanler
                                                                  where e.Status == Status.Employer && e.Salary == query[i].Salary
                                                                  select e;
                                                    break;
                                                case 4:
                                                    secilielan1 = from e in elanler
                                                                  where e.Status == Status.Employer && e.Experience == query[i].Experience
                                                                  select e;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            foreach (var el in secilielan1)
                                            {
                                                if (el.JobTitle != null)
                                                {
                                                    Console.WriteLine($"{++count}) Category : {el.Category} - Position name : {el.JobTitle} - Salary : {el.Salary} ");
                                                    Console.WriteLine("---------------------------------------------");
                                                }
                                            }
                                            if (count != 0)
                                            {
                                                count = 0;
                                                Console.WriteLine("Please enter the number of an announcement");
                                                int.TryParse(Console.ReadLine(), out int y);
                                                Console.Clear();
                                                foreach (var el in secilielan1)
                                                {
                                                    if (el.JobTitle != null)
                                                    {
                                                        ++count;
                                                        if (count == y)
                                                        {
                                                            Console.WriteLine(
                                                            $" Category : {el.Category}  " +
                                                            Environment.NewLine +
                                                                $"Position name : {el.JobTitle}  " +
                                                            Environment.NewLine +
                                                                $"Company Name : {el.CompanyName}  " +
                                                            Environment.NewLine +
                                                                $"Job description : {el.Description}  " +
                                                            Environment.NewLine +
                                                                $"Region : {el.City}  " +
                                                            Environment.NewLine +
                                                                $"Age : {el.Age}  " +
                                                            Environment.NewLine +
                                                                $"Education : {el.Education}  " +
                                                            Environment.NewLine +
                                                                $"Experience : {el.Experience}  " +
                                                            Environment.NewLine +
                                                                $"Phone number: {el.Phone}  " +
                                                            Environment.NewLine +
                                                                $"Salary : {el.Salary} "
                                                                );

                                                            Console.WriteLine("Want to apply? (y/n)");
                                                            string muraciet = Console.ReadLine();
                                                            if (muraciet == "y" || muraciet == "Y")
                                                            {
                                                                AddMuraciet(el, query[i], persons.IndexOf(el));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else { Console.WriteLine("Announcement not found"); }
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        case 4:
                                            Console.Clear();
                                            Console.WriteLine(
                                                $"Username : {query[i].Username} " +
                                                    Environment.NewLine +
                                                $"First Name : {query[i].Name} " +
                                                    Environment.NewLine +
                                                $"Second Name : {query[i].SurName}  " +
                                                    Environment.NewLine +
                                                $"Age : {query[i].Age}  " +
                                                    Environment.NewLine +
                                                $"Gender : {query[i].Gender} " +
                                                    Environment.NewLine +
                                                $"Education : {query[i].Education}  " +
                                                    Environment.NewLine +
                                                $"Experience : {query[i].Experience}  " +
                                                    Environment.NewLine +
                                                $"Category : {query[i].Category}  " +
                                                    Environment.NewLine +
                                                $"Region : {query[i].City}  " +
                                                    Environment.NewLine +
                                                $"Salary : {query[i].Salary}  " +
                                                    Environment.NewLine +
                                                $"Phone number : {query[i].Phone} "
                                                );
                                            Thread.Sleep(5000);
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        case 5:
                                            Console.Clear();
                                            int add = 0;
                                            var allannouncement = from e in elanler
                                                                  where e.Status == Status.Employer
                                                                  select e;
                                            foreach (var elans in allannouncement)
                                            {
                                                if (elans.JobTitle != null)
                                                {
                                                    Console.WriteLine($"{++add}) Category : {elans.Category} " +
                                                        Environment.NewLine +
                                                        $"Position name : {elans.JobTitle}  " +
                                                        Environment.NewLine +
                                                        $"Salary : {elans.Salary} "
                                                        );
                                                }
                                            }
                                            if (add != 0)
                                            {
                                                add = 0;
                                                Console.WriteLine("Please enter the number of an announcement");
                                                int.TryParse(Console.ReadLine(), out int y);
                                                Console.Clear();
                                                foreach (var el in allannouncement)
                                                {
                                                    ++add;
                                                    if (add == y)
                                                    {
                                                        Console.WriteLine(
                                                        $" Category : {el.Category}  " +
                                                        Environment.NewLine +
                                                            $"Position name : {el.JobTitle}  " +
                                                        Environment.NewLine +
                                                            $"Company name : {el.CompanyName}  " +
                                                        Environment.NewLine +
                                                            $"Job description : {el.Description}  " +
                                                        Environment.NewLine +
                                                            $"Region : {el.City}  " +
                                                        Environment.NewLine +
                                                            $"Age : {el.Age}  " +
                                                        Environment.NewLine +
                                                            $"Education : {el.Education}  " +
                                                        Environment.NewLine +
                                                            $"Experience : {el.Experience}  " +
                                                        Environment.NewLine +
                                                            $"Phone number: {el.Phone}  " +
                                                        Environment.NewLine +
                                                            $"Salary : {el.Salary} "
                                                            );

                                                        Console.WriteLine("Want to apply? (y/n)");
                                                        string muraciet = Console.ReadLine();
                                                        if (muraciet == "y" || muraciet == "Y")
                                                        {
                                                            AddMuraciet(el, query[i], persons.IndexOf(el));
                                                        }
                                                    }
                                                }
                                            }
                                            else { Console.WriteLine("Announcement not found"); }
                                            yoxladiq = persons.Contains(query[i]);
                                            break;
                                        default:
                                            break;
                                    }
                                } while (false);
                            }
                        } while (yoxladiq);
                        break;
                    case 2:
                        var query2 = from e in persons
                                     where e.Username == username && e.Password == pass
                                     select e;
                        var newList5 = query2.OfType<Employer>().Cast<Employer>().ToList();
                        foreach (var item in newList5)
                        {
                            int yoxlama = 0;
                            do
                            {
                                Console.WriteLine($"Hello {item.Username}");
                                Console.WriteLine("1. Add job or change");
                                Console.WriteLine("2. Applicants");
                                Console.WriteLine("0. Çıxış");
                                int.TryParse(Console.ReadLine(), out yoxlama);
                                Console.Clear();
                                switch (yoxlama)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        Console.Clear();
                                        var change = ChangeHR(item);

                                        persons.Add(change);
                                        persons.RemoveAt(persons.IndexOf(item));
                                        var jsons = JsonConvert.SerializeObject(persons, settings);

                                        using (StreamWriter writer = new StreamWriter("file.json"))
                                        {
                                            writer.WriteLine(jsons);
                                        }
                                        break;
                                    case 2:
                                        Console.Clear();
                                        Console.WriteLine("Applicants to the announcement : ");
                                        foreach (var m in item.Worker)
                                        {
                                            Console.WriteLine(
                                                $"First Name : {m.Name} " +
                                                    Environment.NewLine +
                                                $"Second Name : {m.SurName}  " +
                                                    Environment.NewLine +
                                                $"Age : {m.Age}  " +
                                                    Environment.NewLine +
                                                $"Gender : {m.Gender} " +
                                                    Environment.NewLine +
                                                $"Education : {m.Education}  " +
                                                    Environment.NewLine +
                                                $"Experience : {m.Experience}  " +
                                                    Environment.NewLine +
                                                $"Category : {m.Category}  " +
                                                    Environment.NewLine +
                                                $"Region : {m.City}  " +
                                                    Environment.NewLine +
                                                $"Salary : {m.Salary}  " +
                                                    Environment.NewLine +
                                                $"Phone number : {m.Phone} "
                                                );
                                            Console.WriteLine("---------------------------------------------");
                                            Thread.Sleep(2000);
                                        }
                                        break;
                                }
                            } while (yoxlama != 1 || yoxlama != 2);
                        }
                        break;
                    default:
                        Console.WriteLine("Are you playing game?)))"); Thread.Sleep(2000);
                        break;
                }
            } while (choose != 1 && choose != 2);
        }


        public void AddMuraciet(Employer el, Worker list, int x)
        {

            Worker yeni = new Worker()
            {
                Name = list.Name,
                Email = list.Email,
                SurName = list.SurName,
                Phone = list.Phone,
                Gender = list.Gender,
                Age = list.Age,
                Education = list.Education,
                Experience = list.Experience,
                Category = list.Category,
                City = list.City,
                Salary = list.Salary
            };
            el.Worker.Add(yeni);

            persons.Add(el);
            persons.RemoveAt(x);

            var json = JsonConvert.SerializeObject(persons, settings);

            using (StreamWriter writer = new StreamWriter("file.json"))
            {
                writer.WriteLine(json);
            }
        }

        public Worker ChangeCV(Worker list)
        {

            string doqru = null; string num = null;
            Console.WriteLine("First Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Second Name:");
            string surName = Console.ReadLine();
            Console.WriteLine("Cins (Men (0) , Woman (1)):");
            int.TryParse(Console.ReadLine(), out int cins);
            Gender gender = Gender.Men;
            switch (cins)
            {
                case 0:
                    gender = Gender.Men;
                    break;
                case 1:
                    gender = Gender.Woman;
                    break;
                default:
                    break;
            }
            Console.WriteLine("Age :");
            string age = Console.ReadLine();
            Console.WriteLine("Education (Secondary Technical, Bachelor degree, Master degree):");
            string education = Console.ReadLine();
            Console.WriteLine(@"Work Experience { Less than 1 year (0), From 1 to 3 years(1), From 3 to 5 years(2), More than 5 years(3) }:");
            int.TryParse(Console.ReadLine(), out int experience);
            string experiences = null;
            switch (experience)
            {
                case 0:
                    experiences = "Less than 1 year";
                    break;
                case 1:
                    experiences = "From 1 to 3 years";
                    break;
                case 2:
                    experiences = "From 3 to 5 years";
                    break;
                case 3:
                    experiences = "More than 5 years";
                    break;
                default:
                    break;
            }
            Console.WriteLine(@"Category : { IT, HR, Doctor, Journalist, Translator }");
            string category = Console.ReadLine();
            Category value = Category.IT;
            switch (category.ToUpper())
            {
                case "IT":
                    value = Category.IT;
                    break;
                case "HR":
                    value = Category.HR;
                    break;
                case "DOCTOR":
                    value = Category.DOCTOR;
                    break;
                case "JOURNALIST":
                    value = Category.JOURNALIST;
                    break;
                case "TRANSLATOR":
                    value = Category.TRANSLATOR;
                    break;
                default:
                    break;
            }
            Console.WriteLine("Region :");
            string city = Console.ReadLine();
            Console.WriteLine("Salary :");
            string salary = Console.ReadLine();


            do
            {
                num = Generate.GenerateNumber(4);
                Console.WriteLine(num);
                Console.WriteLine("Please enter the above text correctly :");
                doqru = Console.ReadLine();
            } while (doqru != num);


            list.Name = name;
            list.SurName = surName;
            list.Gender = gender;
            list.Age = age;
            list.Education = education;
            list.Experience = experiences;
            list.Category = value;
            list.City = city;
            list.Salary = salary;

            return list;
        }


        public Employer ChangeHR(Employer list)
        {
            string doqru = null; string num = null;
            Console.WriteLine("Company name:");
            string companyName = Console.ReadLine();
            Console.WriteLine("Position name:");
            string jobTitle = Console.ReadLine();
            Console.WriteLine("Job description:");
            string description = Console.ReadLine();
            Console.WriteLine("Region :");
            string city = Console.ReadLine();
            Console.WriteLine("Salary :");
            string salary = Console.ReadLine();
            Console.WriteLine("Age :");
            string age = Console.ReadLine();
            Console.WriteLine("Education (Secondary Technical, Bachelor degree, Master degree):");
            string education = Console.ReadLine();
            Console.WriteLine(@"Work Experience { Less than 1 year (0), From 1 to 3 years(1), From 3 to 5 years(2), More than 5 years(3) }:");
            int.TryParse(Console.ReadLine(), out int experience);
            string experiences = null;
            switch (experience)
            {
                case 0:
                    experiences = "Less than 1 year";
                    break;
                case 1:
                    experiences = "From 1 to 3 years";
                    break;
                case 2:
                    experiences = "From 3 to 5 years";
                    break;
                case 3:
                    experiences = "More than 5 years";
                    break;
                default:
                    break;
            }
            Console.WriteLine("Category :");
            string category = Console.ReadLine();
            Category value = Category.IT;
            switch (category.ToUpper())
            {
                case "IT":
                    value = Category.IT;
                    break;
                case "HR":
                    value = Category.HR;
                    break;
                case "DOCTOR":
                    value = Category.DOCTOR;
                    break;
                case "JOURNALIST":
                    value = Category.JOURNALIST;
                    break;
                case "TRANSLATOR":
                    value = Category.TRANSLATOR;
                    break;
                default:
                    break;
            }

            do
            {
                num = Generate.GenerateNumber(4);
                Console.WriteLine(num);
                Console.WriteLine("Please enter the text correctly :");
                doqru = Console.ReadLine();
            } while (doqru != num);


            list.CompanyName = companyName;
            list.Category = value;
            list.Status = Status.Employer;
            list.JobTitle = jobTitle;
            list.Description = description;
            list.City = city;
            list.Salary = salary;
            list.Age = age;
            list.Education = education;
            list.Experience = experiences;

            return list;
        }



        public void Reg()
        {
            List<Person> persons = new List<Person>();
            Person person;
            FileInfo fileInfo = new FileInfo("file.json");
            if (fileInfo.Exists)
            {
                using (StreamReader red = new StreamReader("file.json"))
                {
                    var new_persons = JsonConvert.DeserializeObject<List<Person>>(red.ReadToEnd(), settings);
                    persons = new_persons;
                }
            }
            var emailRegex = @"^[a-z0-9][-a-z0-9._]+@([-a-z0-9]+\.)+[a-z]{2,5}$";
            var numberRegex = @"^(\+994){4}(?:[\s]?)(?<50>\d{2})|(?:[\s]?)(?<51>\d{2})|(?:[\s]?)(?<55>\d{2})|(?:[\s]?)(?<70>\d{2})|(?:[\s]?)(?<77>\d{2})(\d+){7}$";
            var passRegax = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$";

            string doqru = null; string num = null;
            string username = null;
            string pass = null;
            string email = null;
            foreach (var item in persons)
            {
                bool var = true;
                do
                {
                    Console.WriteLine("Username:");
                    username = Console.ReadLine();
                    if (item.Username == username)
                    {
                        Console.WriteLine("This name is used!");
                        var = true;
                    }
                    else
                        var = false;
                } while (var);
                do
                {
                    Console.WriteLine("Email:");
                    email = Console.ReadLine();
                } while ((!Regex.IsMatch(email, emailRegex)) && (item.Email == email));


                do
                {
                    Console.WriteLine("Password: (1 big, 1 small, 1 character, 1 must be number. Length 8-16)");
                    pass = Console.ReadLine();
                } while (!Regex.IsMatch(pass, passRegax));
                string repass;
                do
                {
                    Console.WriteLine("RePassword: (1 big, 1 small, 1 character, 1 must be number. Length 8-16)");
                    repass = Console.ReadLine();
                } while (pass != repass);

            }

            Console.WriteLine("Status (0-Worker, 1-Employer):");
            int.TryParse(Console.ReadLine(), out int choose);
            string phone;
            do
            {
                Console.WriteLine("Phone number`s (+994 50/51/55/70/77 5555555(7) example (+994 50 5555555) . ):");
                phone = Console.ReadLine();
            } while (!Regex.IsMatch(phone, numberRegex));


            switch (choose)
            {
                case 0:
                    do
                    {
                        num = Generate.GenerateNumber(4);
                        Console.WriteLine(num);
                        Console.WriteLine("Please enter the above text correctly :");
                        doqru = Console.ReadLine();
                    } while (doqru != num);

                    person = new Worker()
                    {
                        Username = username,
                        Password = pass,
                        Email = email,
                        Status = Status.Worker,
                        Phone = phone
                    };

                    persons.Add(person);
                    var json1 = JsonConvert.SerializeObject(persons, settings);

                    using (StreamWriter writer = new StreamWriter("file.json"))
                    {
                        writer.WriteLine(json1);
                    }
                    break;
                case 1:
                    do
                    {
                        num = Generate.GenerateNumber(4);
                        Console.WriteLine(num);
                        Console.WriteLine("Please enter the above text correctly :");
                        doqru = Console.ReadLine();
                    } while (doqru != num);

                    person = new Employer()
                    {
                        Username = username,
                        Password = pass,
                        Email = email,
                        Status = Status.Employer,
                        Phone = phone
                    };

                    persons.Add(person);
                    var json = JsonConvert.SerializeObject(persons, settings);

                    using (StreamWriter writer = new StreamWriter("file.json"))
                    {
                        writer.WriteLine(json);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
