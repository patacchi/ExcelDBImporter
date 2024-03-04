using ExcelDBImporter.Modeles;
using ExcelDBImporter.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;


namespace ExcelDBImporter
{
    public partial class FrmDataViewer : Form
    {
        private readonly ExcelDbContext dbContextAlias;
        public FrmDataViewer()
        {
            InitializeComponent();
            dbContextAlias = new();
        }

        private void FrmDataViewer_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                //dbContextAlias = new();
                //EnsureCreated使うとMigrationの時にタイヘン・・・
                //dbContextAlias.Database.EnsureCreated();
                Tool.RegistAllClassAndPropertys RegistClass = new("ExcelDBImporter.Modeles");
                dgvUpdater.DataSource = dbContextAlias.TableFieldAliasNameLists
                                                        .OrderBy(t => t.StrClassName)
                                                        .ThenBy(t => t.StrColumnName)
                                                        .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
            }
        }

        private void BtnSaveChanges_Click(object sender, EventArgs e)
        {
            MessageBox.Show(dbContextAlias.SaveChanges().ToString() + " 件更新しました");
        }
    }
}
