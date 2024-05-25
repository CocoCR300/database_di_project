using Microsoft.EntityFrameworkCore;
using Restify.API.Models;

namespace Restify.API.Data
{
	public class RestifyDbContext : DbContext
	{
		public const string DATE_FORMAT = "yyyy-MM-dd";
		
		public DbSet<Booking> Booking { get; set; }
		public DbSet<Lodging> Lodging { get; set; }
		public DbSet<Perk> Perks { get; set; }
		public DbSet<Room> Room { get; set; }
		public DbSet<RoomBooking> RoomBooking { get; set; }
		public DbSet<User> User { get; set; }
		
		public RestifyDbContext() { }
		public RestifyDbContext(DbContextOptions<RestifyDbContext> options) : base(options) { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				string connectionString = "server=localhost;user=root;password=;database=restify_v2";
				optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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
					.HasAnnotation("EmailAddress", null)
					.HasMaxLength(150);

				person.HasOne(p => p.User)
					.WithOne(u => u.Person)
					.HasForeignKey<Person>(p => p.UserName);

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
				
				payment.Property(p => p.InvoiceImageFileName)
					.IsRequired()
					.HasMaxLength(75);

				payment.HasOne<Booking>()
					.WithOne(b => b.Payment)
					.HasForeignKey<Payment>(p => p.BookingId)
					.IsRequired(false);
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
