$: << './'
require 'lib/albacore'
require 'version_bumper'

task :default => ["castle:build"]

namespace :castle do

  desc "build project"
  msbuild :build, [:config, :framework] => [:version] do |msb, args|
    msb.use :args[:framework] || :net40
	msb.properties :Configuration => args[:config] || :Release
    msb.targets :Clean, :Build
    msb.solution = 
  end
  
  desc "create the nuget package"
  nuspec do |nuspec|
    nuspec.id = "Castle.Facilities.NHibernate"
    nuspec.version = File.Read
    nuspec.authors = "Henrik Feldt"
    nuspec.description = %q{aaWhen using NHibernate this Castle NHibernate Facility will make it easier to manage sessions and session factories. 
		Integrated with Windsor, Castle Transaction Services, Castle AutoTx Facility and FluentNHibernate, 
		it gives you ease of configuration and many extensibility options.}
    nuspec.title = "Castle NHibernate Facility"
	nuspec.dependency "Castle.Core", "2.5.1"
	nuspec.dependency "Castle.Windsor", "2.5.1"
	nuspec.dependency "Castle.Services.Transaction", "2.5.1"
    nuspec.dependency "Castle.Facilities.AutoTx", "2.5.1"
    nuspec.language = "en-US"
    nuspec.licenseUrl = "http://me.com/license"
    nuspec.projectUrl = "http://me.com"
    nuspec.dependency "Autofac", "2.4.3.700"
    nuspec.working_directory = "Build/Deploy"
    nuspec.output_file = "fluentworkflow.nuspec"
  end
  
end