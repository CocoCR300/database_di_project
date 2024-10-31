using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using Restify.API.Models;

namespace Restify.API.Data
{
	public class RestifyDbContext : DbContext
	{
		public const string SERVER_WIDE_SERVICE_NAME = "ServerWideRestifyDbContext";
		public const string DATABASE_NAME = "restify";
		public const string DATE_FORMAT = "yyyy-MM-dd";
		
		public DbSet<Booking> Booking { get; set; }
		public DbSet<Lodging> Lodging { get; set; }
		public DbSet<PaymentInformation> PaymentInformation { get; set; }
		public DbSet<Perk> Perks { get; set; }
		public DbSet<Room> Room { get; set; }
		public DbSet<RoomBooking> RoomBooking { get; set; }
		public DbSet<User> User { get; set; }

		public RestifyDbContext() { }
		public RestifyDbContext(DbContextOptions<RestifyDbContext> options) : base(options) { }

		public Task CreateDatabaseBackup(string backupName, string backupFile)
		{
			return ExecuteStoredProcedure("Restify.dbo.paCrearPuntoRestauracion",
				CreateInputSqlParameter("@backupName", backupName),
				CreateInputSqlParameter("@backupFile", backupFile)
			);
		}
		
		public Task RestoreDatabase(string backupFile)
		{
			return ExecuteStoredProcedure("Restify.dbo.paRestaurarBaseDatos",
				CreateInputSqlParameter("@backupFile", backupFile)
			);
		}

		public async Task<int> InsertBooking(string userName, Booking booking)
		{
			SqlMetaData[] tableSchema = new SqlMetaData[]
			{
				new SqlMetaData("roomNumber", SqlDbType.Int),
				new SqlMetaData("cost", SqlDbType.Decimal),
				new SqlMetaData("fees", SqlDbType.Decimal),
				new SqlMetaData("discount", SqlDbType.Decimal),
				new SqlMetaData("startDate", SqlDbType.VarChar, 10),
				new SqlMetaData("endDate", SqlDbType.VarChar, 10)
			};

			//And a table as a list of those records
			List<SqlDataRecord> table = new List<SqlDataRecord>();

			foreach (RoomBooking roomBooking in booking.RoomBookings)
			{
				var tableRow = new SqlDataRecord(tableSchema);
				tableRow.SetInt32(0, roomBooking.RoomNumber);
				tableRow.SetDecimal(1, roomBooking.Cost);
				tableRow.SetDecimal(2, roomBooking.Fees);
				tableRow.SetDecimal(3, roomBooking.Discount);
				tableRow.SetString(4, roomBooking.StartDate.ToString("O"));
				tableRow.SetString(5, roomBooking.EndDate.ToString("O"));
				table.Add(tableRow);
			}

			SqlParameter bookingId = CreateOutputSqlParameter("@bookingId", SqlDbType.Int);
			int returnCode = await ExecuteStoredProcedure("paCrearReservacion",
				CreateInputSqlParameter("@userName", userName),
				CreateInputSqlParameter("@lodgingId", booking.LodgingId),
				new SqlParameter("@roomBookings", SqlDbType.Structured)
				{
					Direction = ParameterDirection.Input,
					TypeName = "RoomBookingList",
					Value = table
				},
				bookingId);

			if (returnCode == 0)
			{
				booking.Id = (int) bookingId.Value;
			}

			return returnCode;
		}

		public async Task<int> InsertLodging(Lodging lodging)
		{
			SqlParameter lodgingId = CreateOutputSqlParameter("@lodgingId", SqlDbType.Int);
			int returnCode = await ExecuteStoredProcedure("paCrearAlojamiento",
				CreateInputSqlParameter("@ownerId", lodging.OwnerId),
				CreateInputSqlParameter("@lodgingType", lodging.Type),
				CreateInputSqlParameter("@name", lodging.Name),
				CreateInputSqlParameter("@address", lodging.Address),
				CreateInputSqlParameter("@description", lodging.Address),
				CreateInputSqlParameter("@emailAddress", lodging.Address),
				lodgingId);

			if (returnCode == 0)
			{
				lodging.Id = (int) lodgingId.Value;
			}

			return returnCode;
		}

		public async Task<int> InsertLodgingWithoutRooms(Lodging lodging,
			decimal perNightPrice, decimal fees, int capacity)
		{
			SqlParameter lodgingId = CreateOutputSqlParameter("@lodgingId", SqlDbType.Int);
			int returnCode = await ExecuteStoredProcedure("paCrearAlojamientoSinHabitaciones",
				CreateInputSqlParameter("@ownerId", lodging.OwnerId),
				CreateInputSqlParameter("@lodgingType", lodging.Type.ToString()),
				CreateInputSqlParameter("@name", lodging.Name),
				CreateInputSqlParameter("@address", lodging.Address),
				CreateInputSqlParameter("@description", lodging.Description),
				CreateInputSqlParameter("@emailAddress", lodging.EmailAddress),
				CreateInputSqlParameter("@perNightPrice", perNightPrice),
				CreateInputSqlParameter("@fees", fees),
				CreateInputSqlParameter("@capacity", capacity),
				lodgingId
			);

			if (returnCode == 0)
			{
				lodging.Id = (int) lodgingId.Value;
			}
			
			return returnCode;
		}

		public Task<int> DeleteLodging(int lodgingId)
		{
			return ExecuteStoredProcedure("paEliminarAlojamiento",
				CreateInputSqlParameter("@lodgingId", lodgingId));
		}
		
		public Task<int> InsertUserAndPerson(User user)
		{
			return ExecuteStoredProcedure("paInsertarUsuarioYPersona", 
				CreateInputSqlParameter("@userName", user.Name),
				CreateInputSqlParameter("@userRoleId", user.RoleId),
				CreateInputSqlParameter("@password", user.Password),
				CreateInputSqlParameter("@firstName", user.Person.FirstName),
				CreateInputSqlParameter("@lastName", user.Person.LastName),
				CreateInputSqlParameter("@emailAddress", user.Person.EmailAddress)
			);
		}
		
		public Task<int> DeleteUserAndPerson(string userName)
		{
			return ExecuteStoredProcedure("paEliminarUsuarioYPersona",
				CreateInputSqlParameter("@userName", userName));
		}

		public async Task<int> RegisterPayment(Payment payment)
		{
			SqlParameter paymentId = CreateOutputSqlParameter("@paymentId", SqlDbType.Int);
			SqlParameter paymentAmount = CreateOutputSqlParameter("@paymentAmount", SqlDbType.Decimal);
			SqlParameter dateAndTime = CreateOutputSqlParameter("@dateAndTime", SqlDbType.DateTime);
			int returnCode = await ExecuteStoredProcedure("paRealizarPago",
				CreateInputSqlParameter("@bookingId", payment.BookingId),
				CreateInputSqlParameter("@paymentInformationId", payment.PaymentInformationId),
				paymentId,
				paymentAmount,
				dateAndTime
			);

			if (returnCode == 0)
			{
				payment.Id = (int) paymentId.Value;
				payment.Amount = (decimal) paymentAmount.Value;
				payment.DateAndTime = (DateTime) dateAndTime.Value;
			}
			
			return returnCode;
		}

		public IQueryable<PaymentInformation> GetUserPaymentInformation(string userName)
		{
			return PaymentInformation.FromSql($"SELECT * FROM fnObtenerInformacionPagoUsuario({userName})");
		}

		public async Task<int> InsertPaymentInformation(string userName, PaymentInformation paymentInformation)
		{
			SqlParameter paymentInformationId = CreateOutputSqlParameter("@paymentInformationId",
				SqlDbType.Int);
			
			int returnCode = await ExecuteStoredProcedure("paInsertarInformacionPago",
				CreateInputSqlParameter("@userName", userName),
				CreateInputSqlParameter("@cardNumber", paymentInformation.CardNumber),
				CreateInputSqlParameter("@cardExpiryDate", paymentInformation.CardExpiryDate),
				CreateInputSqlParameter("@cardHolderName", paymentInformation.CardHolderName),
				CreateInputSqlParameter("@cardSecurityCode", paymentInformation.CardSecurityCode),
				paymentInformationId);

			if (returnCode == 0)
			{
				paymentInformation.Id = (int) paymentInformationId.Value;
			}
			
			return returnCode;
		}

		private SqlParameter CreateInputSqlParameter(string parameterName, object value)
		{
			return new SqlParameter
			{
				Direction = ParameterDirection.Input,
				ParameterName = parameterName,
				Value = value
			};
		}
		
		private SqlParameter CreateOutputSqlParameter(string parameterName, SqlDbType parameterType)
		{
			return new SqlParameter(parameterName, parameterType)
			{
				Direction = ParameterDirection.Output
			};
		}

		private async Task<int> ExecuteStoredProcedure(string procedureName,
			params SqlParameter[] parameters)
		{
			SqlParameter returnValue = new SqlParameter("@returnValue", SqlDbType.Int)
			{
				Direction = ParameterDirection.ReturnValue
			};
			
			using (var command = Database.GetDbConnection().CreateCommand())
			{
				command.CommandText = procedureName;
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(returnValue);
				command.Parameters.AddRange(parameters);

				Database.OpenConnection();
				await command.ExecuteNonQueryAsync();
				return (int) returnValue.Value;
			}
		}
		
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				string connectionString = "Integrated Security=True;Initial Catalog=restify;Server=Catalog";
				optionsBuilder.UseSqlServer(connectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserRole>(userRole =>
			{
				userRole.HasKey(u => u.Id);

				userRole.Property(u => u.Id)
					.HasColumnName("userRoleId")
					.IsRequired();

				userRole.Property(u => u.Type)
					.IsRequired()
					.HasMaxLength(50);
			});

			modelBuilder.Entity<User>(user =>
			{
				user.HasKey(u => u.Name);

				user.Property(ar => ar.Name)
					.HasColumnName("userName")
					.IsRequired()
					.HasMaxLength(50);
				
				user.Property(u => u.RoleId)
					.HasColumnName("userRoleId")
					.IsRequired();

				user.Property(ar => ar.Password)
					.IsRequired()
					.HasMaxLength(100);

				user.HasOne(u => u.Role).WithMany().OnDelete(DeleteBehavior.Restrict);
				user.Navigation(u => u.Person).AutoInclude();

				user.ToTable(tb => tb.HasTrigger("disRegistrarInsercionUsuarioAdministrador"));
			});

			modelBuilder.Entity<PhoneNumber>(phoneNumber =>
			{
				phoneNumber.HasNoKey();
				phoneNumber.UseTpcMappingStrategy();
			});
			modelBuilder.Entity<Photo>(photo =>
			{
				photo.HasNoKey();
				
				photo.Property(p => p.FileName)
					.IsRequired()
					.HasColumnType("CHAR(75)");
				
				photo.Property(p => p.Ordering)
					.IsRequired();
				
				photo.UseTpcMappingStrategy();
			});

			modelBuilder.Entity<Person>(person =>
			{
				person.HasKey(p => p.Id);

				person.Property(p => p.Id)
					.HasColumnName("personId")
					.IsRequired();

				person.Property(p => p.UserName)
					.HasColumnName("userName")
					.IsRequired()
					.HasMaxLength(50);

				person.Property(p => p.FirstName)
					.IsRequired()
					.HasMaxLength(50);

				person.Property(p => p.LastName)
					.IsRequired()
					.HasMaxLength(100);

				person.Property(p => p.EmailAddress)
					.IsRequired()
					.HasMaxLength(150);

				person.HasOne(p => p.User)
					.WithOne(u => u.Person)
					.HasForeignKey<Person>(p => p.UserName);

				person.HasMany(p => p.PaymentInformations)
					.WithOne()
					.HasForeignKey(p => p.PersonId);

				person.OwnsMany(p => p.PhoneNumbers,
					phoneNumber =>
					{
						// TODO: Is not really like this in the DB, but it is required for this to work
						phoneNumber.HasKey(p => new { p.PersonId, p.Number });
						
						phoneNumber.Property(p => p.PersonId)
							.IsRequired();

						phoneNumber.Property(p => p.Number)
							.IsRequired()
							.HasColumnType("CHAR(30)")
							.HasColumnName("phoneNumber");
					});
				});

			modelBuilder.Entity<Perk>(perk =>
			{
				perk.HasKey(p => p.Id);

				perk.Property(p => p.Id)
					.IsRequired()
					.HasColumnName("perkId");

				perk.Property(p => p.Name)
					.IsRequired()
					.HasMaxLength(50);

				perk.ToTable("Perk");
			});

			modelBuilder.Entity<Lodging>(lodging =>
			{
				lodging.HasKey(l => l.Id);

				lodging.Property(l => l.Id)
					.IsRequired()
					.HasColumnName("lodgingId");

				lodging.Property(l => l.OwnerId)
					.IsRequired()
					.HasColumnName("ownerPersonId");

				lodging.Property(l => l.Type)
					.IsRequired()
					.HasColumnName("lodgingType")
					.HasColumnType("CHAR(50)")
					.HasConversion<string>();

				lodging.Property(l => l.Name)
					.IsRequired()
					.HasMaxLength(100);

				lodging.Property(l => l.Address)
					.IsRequired()
					.HasMaxLength(300);

				lodging.Property(l => l.Description)
					.IsRequired()
					.HasMaxLength(1000);
				
				lodging.Property(l => l.EmailAddress)
					.IsRequired()
					.HasMaxLength(200);
				
				lodging.OwnsMany(l => l.PhoneNumbers,
					phoneNumber =>
					{
						// TODO: Is not really like this in the DB, but it is required for this to work
						phoneNumber.HasKey(p => new { p.LodgingId, p.Number });
						
						phoneNumber.Property(p => p.LodgingId)
							.IsRequired();

						phoneNumber.Property(p => p.Number)
							.IsRequired()
							.HasColumnType("CHAR(30)")
							.HasColumnName("phoneNumber");
					});
				
				lodging.OwnsMany(l => l.Photos,
					photo =>
					{
						// TODO: Is not really like this in the database, but it is required for this to work
						photo.HasKey(p => p.FileName);
						
						photo.Property(p => p.LodgingId)
							.IsRequired();
					});

				lodging.HasMany(l => l.Perks)
					.WithMany()
					.UsingEntity<LodgingPerk>(lodgingPerk =>
					{
						lodgingPerk.Property(p => p.LodgingId)
							.IsRequired();
						
						lodgingPerk.Property(p => p.PerkId)
							.IsRequired();
					});

				lodging.HasMany(l => l.Rooms)
					.WithOne(r => r.Lodging)
					.HasForeignKey(r => r.LodgingId)
					.OnDelete(DeleteBehavior.Cascade);

				lodging.HasMany(l => l.RoomTypes)
					.WithOne()
					.HasForeignKey(r => r.LodgingId)
					.OnDelete(DeleteBehavior.Cascade);
				
				lodging.HasOne(l => l.Owner)
					.WithMany()
					.HasForeignKey(l => l.OwnerId)
					.OnDelete(DeleteBehavior.Restrict);

				lodging.ToTable(tb => tb.HasTrigger("disActualizacionTipoAlojamiento"));
			});

			modelBuilder.Entity<Booking>(booking =>
			{
				booking.HasKey(b => b.Id);

				booking.Property(b => b.Id)
					.IsRequired()
					.HasColumnName("bookingId");
				
				booking.Property(b => b.CustomerId)
					.IsRequired()
					.HasColumnName("customerPersonId");

				booking.Property(b => b.LodgingId)
					.IsRequired();
				
				booking.HasMany(r => r.RoomBookings)
					.WithOne(r => r.Booking)
					.HasForeignKey(r => r.BookingId);

				booking.HasOne(b => b.Customer)
					.WithMany()
					.HasForeignKey(b => b.CustomerId)
					.OnDelete(DeleteBehavior.Restrict);

				booking.HasOne(b => b.Lodging)
					.WithMany()
					.HasForeignKey(b => b.LodgingId)
					.OnDelete(DeleteBehavior.Restrict);

				booking.Navigation(b => b.Lodging).AutoInclude(false);
			});

			modelBuilder.Entity<PaymentInformation>(paymentInformation =>
			{
				paymentInformation.HasKey(p => p.Id);

				paymentInformation.Property(p => p.Id)
					.IsRequired()
					.HasColumnName("paymentInformationId");

				paymentInformation.Property(p => p.CardExpiryDate)
					.IsRequired();

				paymentInformation.Property(p => p.CardHolderName)
					.IsRequired()
					.HasMaxLength(100)
					.HasColumnType("CHAR(100)");

				paymentInformation.Property(p => p.CardNumber)
					.IsRequired()
					.HasMaxLength(16)
					.HasColumnType("CHAR(16)");

				paymentInformation.Property(p => p.CardSecurityCode)
					.IsRequired()
					.HasMaxLength(4)
					.HasColumnType("CHAR(4)");

				paymentInformation.Property(p => p.PersonId)
					.IsRequired();
			});

			modelBuilder.Entity<Payment>(payment =>
			{
				payment.HasKey(p => p.Id);

				payment.Property(p => p.Id)
					.IsRequired()
					.HasColumnName("paymentId");

				payment.Property(p => p.BookingId)
					.IsRequired(false);

				payment.Property(p => p.DateAndTime)
					.IsRequired();
				
				payment.Property(p => p.Amount)
					.IsRequired();
				
				payment.Property(p => p.PaymentInformationId)
					.IsRequired();

				payment.HasOne<PaymentInformation>()
					.WithOne()
					.HasForeignKey<Payment>(p => p.PaymentInformationId);

				payment.HasOne<Booking>()
					.WithOne(b => b.Payment)
					.HasForeignKey<Payment>(p => p.BookingId)
					.IsRequired(false);

				payment.ToTable(tb =>
				{
					tb.HasTrigger("disProhibirBorradoPago");
					tb.HasTrigger("disConfirmarReservaAlRegistrarPago");
					tb.HasTrigger("disRegistrarInsercionPago");
					tb.HasTrigger("disRegistrarActualizacionCantidadPago");
				});
			});

			modelBuilder.Entity<RoomType>(roomType =>
			{
				roomType.HasKey(r => r.Id);

				roomType.Property(r => r.Id)
					.IsRequired()
					.HasColumnName("RoomTypeId");
				
				roomType.Property(r => r.LodgingId)
					.IsRequired();

				roomType.Property(r => r.Name)
					.IsRequired()
					.HasMaxLength(75);

				roomType.Property(r => r.Fees)
					.IsRequired();
				
				roomType.Property(r => r.PerNightPrice)
					.IsRequired();

				roomType.Property(r => r.Capacity)
					.IsRequired();

				roomType.OwnsMany(r => r.Photos,
					photo =>
					{
						// TODO: Is not really like this in the database, but it is required for this to work
						photo.HasKey(p => p.FileName);
						
						photo.Property(p => p.RoomTypeId)
							.IsRequired();
					});

				roomType.ToTable(tb => tb.HasTrigger("disTiposHabitacionesEnAlojamientosQueAdmiten"));
			});
			
			modelBuilder.Entity<Room>(room =>
			{
				room.HasKey(r => new  { r.LodgingId, r.Number });

				room.Property(r => r.LodgingId)
					.IsRequired();
				
				room.Property(r => r.TypeId)
					.IsRequired()
					.HasColumnName("roomTypeId");

				room.Property(r => r.Number)
					.IsRequired()
					.HasColumnName("roomNumber");

				room.HasOne(r => r.Type)
					.WithMany()
					.HasForeignKey(r => r.TypeId);

				room.ToTable(tb => tb.HasTrigger("disHabitacionesEnAlojamientosQueAdmiten"));
			});

			modelBuilder.Entity<RoomBooking>(roomBooking =>
			{
				roomBooking.HasKey(r => r.Id);

				roomBooking.Property(r => r.Id)
					.IsRequired()
					.HasColumnName("roomBookingId");

				roomBooking.Property(r => r.BookingId)
					.IsRequired();

				roomBooking.Property(r => r.LodgingId)
					.IsRequired();

				roomBooking.Property(r => r.RoomNumber)
					.IsRequired();

				roomBooking.Property(r => r.Cost)
					.IsRequired();

				roomBooking.Property(r => r.Discount)
					.IsRequired();
				
				roomBooking.Property(r => r.Fees)
					.IsRequired();
				
				roomBooking.Property(r => r.StartDate)
					.IsRequired();
				
				roomBooking.Property(r => r.EndDate)
					.IsRequired();

				roomBooking.Property(b => b.Status)
					.IsRequired()
					.HasColumnType("CHAR(50)")
					.HasConversion<string>();

				roomBooking.HasOne(r => r.Room)
					.WithMany()
					.HasForeignKey(r => new { r.LodgingId, r.RoomNumber });
			});
		}
	}
}
