using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net.NetworkInformation;

class Program
{
    static void Main(string[] args)
    {
        List<Staff> myStaff = new List<Staff>();
        FileReader fr = new FileReader();
        int month = 0;
        int year = 0;

        while(year == 0)
        {
            Console.Write("Please enter the year : ");
            try
            {
                year = Convert.ToInt32(Console.ReadLine());

            }
            catch (FormatException)
            {
                Console.WriteLine("You have typed in the wrong type of value");
                year = 0;
                continue;
            }
        }
        while(month == 0) 
        {
            Console.Write("Please enter the month : ");
            try
            {
                month = Convert.ToInt32(Console.ReadLine());
                if(month < 1 || month > 12)
                {
                    Console.WriteLine("You have typed invalid value");
                    month = 0;

                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        myStaff = fr.ReadFile();
        for (int i = 0; i < myStaff.Count; i++)
        {
            try
            {
                Console.WriteLine("Enter hours worked for {0}", myStaff[i].NameOfStaff);
                int hoursWorked = Convert.ToInt32(Console.ReadLine());
                myStaff[i].HoursWorked = hoursWorked;
                myStaff[i].CalculatePay();
                Console.WriteLine(myStaff[i].ToString());

            }
            catch (Exception e)
            {
                i--;
                Console.WriteLine(e.Message);
            }

        }
        PaySlip ps = new PaySlip(month,year);
        ps.GeneratePaySlip(myStaff);
        ps.GenerateSummary(myStaff);

        Console.ReadLine();

    }
}
class Staff
{
    private float HourlyRate;
    private int hWorked;
    public float TotalPay { get; protected set; }
    public float BasicPay { get; private set; }
    public string NameOfStaff { get; private set; }
    public int HoursWorked { 
        get 
        {
            return hWorked;
        }
        set
        {
            if (value > 0 )
            {
                hWorked = value;
            }
            else
            {
                hWorked = 0;
            }
        }
    }
    public Staff(string name , float rate)
    {
        NameOfStaff = name;
        HourlyRate = rate;
    }
    public virtual void CalculatePay()
    {
        Console.WriteLine("Calculating Pay...");
        BasicPay = hWorked * HourlyRate;
        TotalPay = BasicPay;
    }
    public override string ToString()
    {
        return NameOfStaff + " Workers name " + " his hourly rate equals to = " + HourlyRate
            + " worker has worked " + hWorked + " and his total pay equals = " + TotalPay; 
    }

}

class Manager : Staff
{
    private const int managerHourlyRate = 50;
    public int Allowance { get; private set; }
    public Manager(string name) : base(name, managerHourlyRate) { }
    public override void CalculatePay()
    {
        base.CalculatePay();
        Allowance = 1000;
        if (HoursWorked > 160 ) {
            TotalPay += Allowance;
        }
    }
    public override string ToString()
    {
        return NameOfStaff + " Manager " + " his hourly rate equals to = " + managerHourlyRate
          + " worker has worked " + HoursWorked + " and his total pay equals = " + TotalPay;
    }
}
class Admin : Staff
{
    private const float overtimeRate = 15.5F;
    private const float adminHourlyRate = 30;
    public float OverTime { get; private set; }

    public Admin(string name) : base(name, adminHourlyRate) { }
    public override void CalculatePay()
    {
        base.CalculatePay();
        if(HoursWorked > 160)
        {
            OverTime = overtimeRate * (HoursWorked - 160);
            TotalPay += OverTime;
        }
    }
    public override string ToString()
    {
        return "Admin name " + NameOfStaff + ", admins hourly rate " + adminHourlyRate
            + " his total pay is " + TotalPay;
    }
}
class FileReader
{
    public List<Staff> ReadFile()
    {
        List<Staff> myStaff = new List<Staff>();
        string[] result = new string[2];
        string path = "staff.txt";
        string[] separator = {", "};

        if (File.Exists(path))
        {
            using(StreamReader sr = new StreamReader(path))
            {
                while(!sr.EndOfStream)
                {
                    result = sr.ReadLine().Split(separator[0],StringSplitOptions.None);
                    if (result[1] == "Manager")
                    {
                        Manager manage = new Manager(result[0]);
                        myStaff.Add(manage);
                    }
                    else if (result[1] == "Admin")
                    {
                        Admin admin = new Admin(result[0]);
                        myStaff.Add(admin);
                    }
                }
                sr.Close();
            }
        }
        else
        {
            Console.WriteLine("Could not find such file");
        }
        return myStaff;
    }
}

class PaySlip
{
    private int month, year;
    enum MonthsOfYear
    {
        JAN = 1,FEB = 2,MAR = 3,APR = 4,
        MAY = 5,JUN = 6,JUL = 7,AUG = 8,
        SEP = 9,OCT = 10,NOV = 11,DEC = 12,
    }
    public PaySlip(int payMonth, int payYear)
    {
        month = payMonth;
        year = payYear;
    }
    public void GeneratePaySlip(List<Staff> myStaff)
    {
        string path;
        foreach(Staff f in myStaff)
        {
            path = f.NameOfStaff + ".txt";
            using(StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("PAYSLIP FOR {0} {1}", (MonthsOfYear)month, year);
                sw.WriteLine("==========================");
                sw.WriteLine("Name of Staff: {0}", f.NameOfStaff);
                sw.WriteLine("Basic Pay: {0:C}", f.BasicPay);
                sw.WriteLine("");

                if(f.GetType() == typeof(Manager))
                {
                    sw.WriteLine("Allowance : {0}",((Manager)f).Allowance);
                }
                else
                {
                    sw.WriteLine("Overtime : {0}",((Admin)f).OverTime);
                }
                sw.WriteLine("");
                sw.WriteLine("==========================");
                sw.WriteLine("Total pay : {0}", f.TotalPay);
                sw.WriteLine("==========================");


                sw.Close();
            }
        }
    }
    public void GenerateSummary(List<Staff> myStaff)
    {
        var lessThan10Hours =
            from f in myStaff
            where f.HoursWorked < 10
            orderby f.NameOfStaff ascending
            select new { f.NameOfStaff, f.HoursWorked };

        string path = "summary.txt";
        using(StreamWriter sr = new StreamWriter(path)) 
        {
            sr.WriteLine("Staff with less than 10 working hours");
            sr.WriteLine("");
            foreach(var f in lessThan10Hours)
            {
                sr.WriteLine("Name of Staff: {0}, Hours Worked: {1}", f.NameOfStaff, f.HoursWorked);
            }
            sr.Close();
        }
    }
    public override string ToString()
    {
        return "Month " + month + " year" + year;
    }
}
 