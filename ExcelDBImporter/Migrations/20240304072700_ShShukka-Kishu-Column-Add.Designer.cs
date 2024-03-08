﻿// <auto-generated />
using System;
using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    [DbContext(typeof(ExcelDbContext))]
    [Migration("20240304072700_ShShukka-Kishu-Column-Add")]
    partial class ShShukkaKishuColumnAdd
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0-preview.1.24081.2");

            modelBuilder.Entity("ExcelDBImporter.Models.ShShukka", b =>
                {
                    b.Property<int>("ShShukkaID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DateAssemble")
                        .HasColumnType("TEXT")
                        .HasComment("組立日");

                    b.Property<DateTime?>("DateFunctionTest")
                        .HasColumnType("TEXT")
                        .HasComment("試験日");

                    b.Property<DateTime?>("DateMarshalling")
                        .HasColumnType("TEXT")
                        .HasComment("マーシャリング日");

                    b.Property<DateTime?>("DatePrepare")
                        .HasColumnType("TEXT")
                        .HasComment("出荷準備");

                    b.Property<DateTime?>("DateShippingTest")
                        .HasColumnType("TEXT")
                        .HasComment("出荷検査日");

                    b.Property<DateTime>("DateShukka")
                        .HasColumnType("TEXT")
                        .HasComment("出荷計画");

                    b.Property<DateTime?>("DateShutuzu")
                        .HasColumnType("TEXT")
                        .HasComment("出図日");

                    b.Property<int>("IntAmount")
                        .HasColumnType("INTEGER")
                        .HasComment("発番数量");

                    b.Property<string>("StrHinmei")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasComment("品名");

                    b.Property<string>("StrKishu")
                        .HasColumnType("TEXT")
                        .HasComment("機種");

                    b.Property<string>("StrOrderFrom")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasComment("注文主");

                    b.Property<string>("StrSeiban")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasComment("製番");

                    b.HasKey("ShShukkaID");

                    b.ToTable("ExcelData", t =>
                        {
                            t.HasComment("発番出荷物件予定表モデルクラス");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TableFieldAliasNameList", b =>
                {
                    b.Property<int>("TableFieldAliasNameListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(0);

                    b.Property<string>("StrClassName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1);

                    b.Property<string>("StrColnmnAliasName")
                        .HasColumnType("TEXT");

                    b.Property<string>("StrColumnName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2);

                    b.HasKey("TableFieldAliasNameListId");

                    b.ToTable("TableFieldAliasNameLists", t =>
                        {
                            t.HasComment("テーブル列名の別名(表示名等)格納テーブル");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
