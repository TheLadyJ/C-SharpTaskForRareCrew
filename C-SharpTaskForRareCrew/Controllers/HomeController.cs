using C_SharpTaskForRareCrew.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace C_SharpTaskForRareCrew.Controllers
{
    public class HomeController : Controller
    {
        private readonly string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        public async Task<IActionResult> Index()
        {
            List<EmployeeModel> employeesAPIData = await GetEmployeesAPIData();
            List<EmployeeDTO> employeeDTOs = GetEmployeesDTOData(employeesAPIData);
            return View(employeeDTOs);
        }

        private async Task<List<EmployeeModel>> GetEmployeesAPIData()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<EmployeeModel>>(data);
                }
                else
                {
                    return null;
                }
            }
        }

        private List<EmployeeDTO> GetEmployeesDTOData(List<EmployeeModel> employeesAPIData)
        {
            Dictionary<string, EmployeeDTO> uniqueEmployeesMap = new Dictionary<string, EmployeeDTO>();

            foreach (var employee in employeesAPIData)
            {
                var employeeName = employee.EmployeeName;
                if (employeeName != null)
                {
                    var totalHours = CalculateTotalWorkingHours(employee.StarTimeUtc, employee.EndTimeUtc);
                    if (!uniqueEmployeesMap.ContainsKey(employeeName))
                    {
                        uniqueEmployeesMap.Add(employeeName, new EmployeeDTO
                        {
                            EmployeeName = employeeName,
                            TotalWorkingHours = totalHours
                        });
                    }
                    else
                    {
                        var existingEmployee = uniqueEmployeesMap[employeeName];
                        var existingTotalHours = existingEmployee.TotalWorkingHours;
                        existingEmployee.TotalWorkingHours = existingTotalHours + totalHours;
                    }
                }
            }

            return uniqueEmployeesMap.Values.ToList();
        }

        public static int CalculateTotalWorkingHours(DateTime startTime, DateTime endTime)
        {
            return (int)Math.Round((endTime - startTime).TotalHours);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}