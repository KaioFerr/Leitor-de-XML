using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Leitor_de_XML
{
    public partial class LeitorXML : Form
    {
        public LeitorXML()
        {
            InitializeComponent();

            // Configuração inicial do DataGridView
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "CFOP";
            dataGridView1.Columns[1].Name = "ICMS";
            dataGridView1.Columns[2].Name = "PIS";
            dataGridView1.Columns[3].Name = "COFINS";
            dataGridView1.Columns[4].Name = "IPI";
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // Configuração inicial do DataGridView
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "CFOP";
            dataGridView1.Columns[1].Name = "ICMS";
            dataGridView1.Columns[2].Name = "PIS";
            dataGridView1.Columns[3].Name = "COFINS";
            dataGridView1.Columns[4].Name = "IPI";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        public void ShowText(string[] directories)
        {
            var cfopList = new HashSet<string>();
            var icmsList = new HashSet<string>();
            var pisList = new HashSet<string>();
            var cofinsList = new HashSet<string>();
            var ipiList = new HashSet<string>();

            foreach (string dir in directories)
            {
                try
                {
                    var tributacao = ExtractTributacaoFromXml(dir);
                    cfopList.UnionWith(tributacao.CFOP);
                    icmsList.UnionWith(tributacao.ICMS);
                    pisList.UnionWith(tributacao.PIS);
                    cofinsList.UnionWith(tributacao.COFINS);
                    ipiList.UnionWith(tributacao.IPI);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao processar o arquivo {dir}: {ex.Message}");
                }
            }

            dataGridView1.Rows.Clear(); // Limpa a tabela antes de adicionar novos dados

            DisplayResults(cfopList, icmsList, pisList, cofinsList, ipiList);
        }

        private Tributacao ExtractTributacaoFromXml(string filePath)
        {
            var tributacao = new Tributacao();

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            XmlNodeList itemList = doc.SelectNodes("//nfe:det", nsmgr);

            foreach (XmlNode item in itemList)
            {
                string cfopCod = item.SelectSingleNode("nfe:prod/nfe:CFOP", nsmgr)?.InnerText;
                string origCod = item.SelectSingleNode("nfe:imposto/nfe:ICMS//nfe:orig", nsmgr)?.InnerText;
                string icmsCod = item.SelectSingleNode("nfe:imposto/nfe:ICMS//nfe:CST", nsmgr)?.InnerText ??
                                 item.SelectSingleNode("nfe:imposto/nfe:ICMS//nfe:CSOSN", nsmgr)?.InnerText;
                string pisCod = item.SelectSingleNode("nfe:imposto/nfe:PIS//nfe:CST", nsmgr)?.InnerText;
                string cofinsCod = item.SelectSingleNode("nfe:imposto/nfe:COFINS//nfe:CST", nsmgr)?.InnerText;
                string ipiCod = item.SelectSingleNode("nfe:imposto/nfe:IPI//nfe:CST", nsmgr)?.InnerText;

                if (!string.IsNullOrEmpty(cfopCod))
                    tributacao.CFOP.Add(cfopCod);

                if (!string.IsNullOrEmpty(origCod) && !string.IsNullOrEmpty(icmsCod))
                    tributacao.ICMS.Add(origCod + icmsCod);

                if (!string.IsNullOrEmpty(pisCod))
                    tributacao.PIS.Add(pisCod);

                if (!string.IsNullOrEmpty(cofinsCod))
                    tributacao.COFINS.Add(cofinsCod);

                if (!string.IsNullOrEmpty(ipiCod))
                    tributacao.IPI.Add(ipiCod);
            }

            return tributacao;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void DisplayResults(HashSet<string> cfopList, HashSet<string> icmsList, HashSet<string> pisList, HashSet<string> cofinsList, HashSet<string> ipiList)
        {
            int maxRows = Math.Max(Math.Max(cfopList.Count, icmsList.Count), Math.Max(pisList.Count, cofinsList.Count));
            var cfopEnum = cfopList.GetEnumerator();
            var icmsEnum = icmsList.GetEnumerator();
            var pisEnum = pisList.GetEnumerator();
            var cofinsEnum = cofinsList.GetEnumerator();
            var ipiEnum = ipiList.GetEnumerator();

            for (int i = 0; i < maxRows; i++)
            {
                string cfop = cfopEnum.MoveNext() ? cfopEnum.Current : "";
                string icms = icmsEnum.MoveNext() ? icmsEnum.Current : "";
                string pis = pisEnum.MoveNext() ? pisEnum.Current : "";
                string cofins = cofinsEnum.MoveNext() ? cofinsEnum.Current : "";
                string ipi = ipiEnum.MoveNext() ? ipiEnum.Current : "";

                dataGridView1.Rows.Add(cfop, icms, pis, cofins, ipi);
            }
        }

        public class Tributacao
        {
            public HashSet<string> CFOP { get; } = new HashSet<string>();
            public HashSet<string> ICMS { get; } = new HashSet<string>();
            public HashSet<string> PIS { get; } = new HashSet<string>();
            public HashSet<string> COFINS { get; } = new HashSet<string>();
            public HashSet<string> IPI { get; } = new HashSet<string>();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            // Seleciona múltiplos arquivos XML
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML files (*.xml)|*.xml";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ShowText(openFileDialog.FileNames);
                }
            }
        }
    }
}
