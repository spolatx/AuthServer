﻿using AuthServer.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            //id primary key
            //en best practise yöntem burada tanımlamaktır.
            builder.HasKey(x => x.Id);
            builder.Property(x=>x.Name).IsRequired().HasMaxLength(199);
            builder.Property(x=>x.Price).HasColumnType("decimal(9,2)");
            builder.Property(x => x.UserId).IsRequired();
        }
    }
}
