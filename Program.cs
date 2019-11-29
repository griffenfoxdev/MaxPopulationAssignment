using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace GetMaxPopulationYear
{
    class Program
    {
        //Main 
        static void Main()
        {

            Console.WriteLine("Please enter the full file path for the CSV file to test.");

            string csv_file_path = Console.ReadLine();

            //Get CSV File and generate table
            DataTable csvData = GetDataTableFromCSVFile(csv_file_path);

            //Get year with the highest population
            string MaxPopulationYears = GetMaxPopulationYear(csvData);

            //Show user 
            Console.WriteLine("Year(s) with max population:" + MaxPopulationYears);

            Console.ReadLine();
        }

        //Get user submitted csv file and generat a datatable from it
        private static DataTable GetDataTableFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();

            try
            {

                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Reading File");
            }
            return csvData;
        }


        //Get the year(s) with the highest population of people
        private static string GetMaxPopulationYear(DataTable csvData)
        {


            string maxpopulationyears = "";
            int peoplealive = 0;

            DataTable dtYears = new DataTable();
            dtYears.Columns.Add("Year", typeof(Int32));
            dtYears.Columns.Add("BirthCount", typeof(Int32));
            dtYears.Columns.Add("DeathCount", typeof(Int32));
            dtYears.Columns.Add("PeopleAlive", typeof(Int32));

            DataTable dtSortedYears;

            //Loop through Full population datatable to the amount of people who were born/died in a given year
            foreach (DataRow row in csvData.Rows)
            {

                if (row["birth_date"].ToString() != null && row["birth_date"].ToString() != "")
                {
                    int BirthDate = DateTime.Parse(row["birth_date"].ToString()).Year;

                    DataRow birthdaterow = dtYears.Select("Year='" + BirthDate + "'").FirstOrDefault();

                    if (birthdaterow != null)
                    {
                        int CurrentBirthsInYear = (from DataRow dr in dtYears.Rows
                                                   where (Int32)dr["Year"] == BirthDate
                                                   select (Int32)dr["BirthCount"]).FirstOrDefault();

                        birthdaterow["BirthCount"] = CurrentBirthsInYear + 1;
                    }
                    else
                    {
                        dtYears.Rows.Add(BirthDate, 1, 0);
                    }
                }
                if (row["death_date"].ToString() != null && row["death_date"].ToString() != "")
                {
                    int DeathDate = DateTime.Parse(row["death_date"].ToString()).Year;

                    DataRow deathdaterow = dtYears.Select("Year='" + DeathDate + "'").FirstOrDefault();


                    if (deathdaterow != null)
                    {
                        int CurrentDeathsInYear = (from DataRow dr in dtYears.Rows
                                                   where (Int32)dr["Year"] == DeathDate
                                                   select (Int32)dr["DeathCount"]).FirstOrDefault();

                        deathdaterow["DeathCount"] = CurrentDeathsInYear + 1;
                    }
                    else
                    {
                        dtYears.Rows.Add(DeathDate, 1, 0);
                    }

                }
            }

            //Sort and generate new datatable for getting the number of people alive each year
            dtYears.DefaultView.Sort = "Year ASC";
            dtSortedYears = dtYears.DefaultView.ToTable();

            foreach (DataRow row in dtSortedYears.Rows)
            {
                int peoplebornthisyear = Int32.Parse(row["BirthCount"].ToString());
                int peoplediedthisyear = Int32.Parse(row["DeathCount"].ToString());


                peoplealive += peoplebornthisyear;
                peoplealive -= peoplediedthisyear;

                row["PeopleAlive"] = peoplealive;
            }

            //Select years with the max amount of people alive
            DataRow[] MaxPopulationRow = dtSortedYears.Select("[PeopleAlive] = MAX([PeopleAlive])");

 
            foreach (DataRow row in MaxPopulationRow)
            {
                maxpopulationyears += row["Year"] + " ";
            }

            return maxpopulationyears;
        }


    }
}
