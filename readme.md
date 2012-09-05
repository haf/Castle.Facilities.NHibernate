# Castle NHibernate Facility

Code is currently in Beta stage. I think it's stable enough to use.
Currently compiles against NHibernate 3.1 + FluentNHibernate 1.2. Use the wiki for documentation.

# How to build

 1. Install Ruby 1.9.2
 2. `gem install albacore version_bumper`
 3. `git clone https://haf@github.com/haf/Castle.Facilities.NHibernate.git`
 4. `cd "Castle.Facilities.NHibernate"`
 5. Install an instance of SQL Server named .\SQLEXPRESS
 6. Create a database in this instance named 'CastleTxTests' (for unit tests)
 5. `rake`