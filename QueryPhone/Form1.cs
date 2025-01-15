using QueryPhone.Core.Clients;
using QueryPhone.Core.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryPhone
{
    public partial class Form1 : Form
    {
        private readonly IEnumerable<IQueryPhoneClient> _queryPhoneClients;

        public Form1(IEnumerable<IQueryPhoneClient> queryPhoneClients)
        {
            InitializeComponent();
            _queryPhoneClients = queryPhoneClients;
            foreach (var ins in _queryPhoneClients)
            {
                int i = clientsCheckedListBox.Items.Add(ins.Name);
                clientsCheckedListBox.SetItemChecked(i, true);
            }
        }

        private async void btnQueryPhone_Click(object sender, EventArgs e)
        {
            string phone = GetQueryPhone();
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("請輸入號碼");
                return;
            }

            IEnumerable<IQueryPhoneClient> checkedClients = GetCheckedClients();
            if (!checkedClients.Any())
            {
                MessageBox.Show("請選取查詢來源");
                return;
            }

            txtResult.Clear();
            var tasks = checkedClients.Select(client =>
                Task.Run(async () =>
                {
                    return PrintResult(client.Name, await client.QueryAsync(phone));
                })).ToList();

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                txtResult.AppendText(await completedTask);
            }
        }

        /// <summary> 顯示查詢結果 </summary>
        private static string PrintResult(string name, QueryPhoneResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-------{name}-------");
            sb.AppendLine($"Url：{result.QueryUrl}");
            if (!result.Success)
            {
                sb.AppendLine("查詢錯誤");
                return sb.ToString();
            }

            sb.AppendLine("●用戶回報：");
            sb.AppendLine(string.Join(Environment.NewLine, result.ReportMsgs));
            if (result.ReportMsgs.Any())
                sb.AppendLine();
            sb.AppendLine("●總評：");
            sb.AppendLine(string.Join(Environment.NewLine, result.SummaryMsgs));
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary> 取得查詢電話號碼 </summary>
        private string GetQueryPhone()
        {
            string phone = txtPhone.Text;
            if (string.IsNullOrEmpty(phone))
                return phone;
            return Regex.Replace(phone, @"\D", string.Empty);
        }

        /// <summary> 取得勾選的查詢來源 </summary>
        private IEnumerable<IQueryPhoneClient> GetCheckedClients()
        {
            IEnumerable<string> checkedItemNames = this.clientsCheckedListBox.CheckedItems.OfType<string>();
            return _queryPhoneClients.Join(checkedItemNames, 
                ins => ins.Name, 
                name => name, 
                (ins, name) => ins);
        }

    }
}
