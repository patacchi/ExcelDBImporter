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
    [Migration("20240306063659_ShShukka-Add-output-flag")]
    partial class ShShukkaAddoutputflag
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
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(0);

                    b.Property<DateTime?>("DateAssemble")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(9)
                        .HasComment("組立日");

                    b.Property<DateTime?>("DateFunctionTest")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(10)
                        .HasComment("試験日");

                    b.Property<DateTime?>("DateMarshalling")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(8)
                        .HasComment("マーシャリング日");

                    b.Property<DateTime?>("DatePrepare")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(11)
                        .HasComment("出荷準備");

                    b.Property<DateTime?>("DateShippingTest")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(12)
                        .HasComment("出荷検査日");

                    b.Property<DateTime>("DateShukka")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1)
                        .HasComment("出荷計画");

                    b.Property<DateTime?>("DateShutuzu")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(7)
                        .HasComment("出図日");

                    b.Property<int>("IntAmount")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(6)
                        .HasComment("発番数量");

                    b.Property<bool>("IsAlreadyOutput")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(13)
                        .HasComment("データ出力済みフラグ");

                    b.Property<string>("StrHinmei")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(5)
                        .HasComment("品名");

                    b.Property<string>("StrKishu")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2)
                        .HasComment("機種");

                    b.Property<string>("StrOrderFrom")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(4)
                        .HasComment("注文主");

                    b.Property<string>("StrSeiban")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(3)
                        .HasComment("製番");

                    b.HasKey("ShShukkaID");

                    b.ToTable("ShShukka", t =>
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
