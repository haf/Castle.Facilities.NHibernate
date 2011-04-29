# copyright Henrik Feldt 2011

$: << './'
require 'albacore'
require 'buildscripts/albacore_mods'
begin
  require 'version_bumper'  
rescue LoadError
  puts 'version bumper not available!'
end
require 'rake/clean'
require 'buildscripts/project_data'
require 'buildscripts/paths'
require 'buildscripts/utils'
require 'buildscripts/environment'

# profile time: "PS \> $start = [DateTime]::UtcNow ; rake ; $end = [DateTime]::UtcNow ; $diff = $end-$start ; "Started: $start to $end, a diff of $diff"
task :default => [:release]

desc "prepare the version info files to get ready to start coding!"
task :prepare => ["castle:assembly_infos"]

desc "build in release mode"
task :release => ["env:release", "castle:build", "castle:nuget"]

desc "build in debug mode"
task :debug => ["env:debug", "castle:build"]
 
task :ci => ["clobber", "castle:build_notest", "castle:nuget"]

desc "Run all unit and integration tests in debug mode"
task :test_all => ["env:debug", "castle:test_all"]

desc "prepare alpha version for being published"
task :alpha => ["env:release"] do
  puts %q{
  
    Preparing Alpha Release
	
}
end

CLOBBER.include(Folders[:out])
CLOBBER.include(Folders[:packages])

Albacore.configure do |config|
  config.nunit.command = Commands[:nunit]
  config.assemblyinfo.namespaces = "System", "System.Reflection", "System.Runtime.InteropServices", "System.Security"
end

desc "Builds Debug + Release"
task :build_all do
  ["env:release", "env:debug"].each{ |t| build t }
end

def build(conf)
  puts "BUILD ALL CONF #{conf}"
  Rake::Task.tasks.each{ |t| t.reenable }
  Rake::Task[conf].invoke # these will only be invoked once each
  Rake::Task["castle:build"].invoke
  Rake::Task["castle:test_all"].invoke
end

namespace :castle do

  desc "build + unit tests + output"
  task :build => [:version, :msbuild, :test, :output]
  task :build_notest => [:version, :msbuild, :output]
 
  desc "generate the assembly infos you need to compile with VS"
  task :assembly_infos => [:version]
  
  desc "prepare nuspec + nuget package"
  task :nuget => [:nuget_inner]
  
  task :test_all => [:test]
  
  #                    BUILDING
  # ===================================================
  
  msbuild :msbuild do |msb, args|
    # msb.use = :args[:framework] || :net40
    config = "#{FRAMEWORK.upcase}-#{CONFIGURATION}"
    puts "Building config #{config} with MsBuild"
    msb.properties :Configuration => config
    msb.targets :Clean, :Build
    msb.solution = Files[:sln]
  end
    
  #                    VERSIONING
  #        http://support.microsoft.com/kb/556041
  # ===================================================
  
  file 'src/CommonAssemblyInfo.cs' => "castle:version"
  
  assemblyinfo :version do |asm|
    data = commit_data() #hash + date
    asm.product_name = asm.title = Projects[:nh_fac][:title]
    asm.description = Projects[:nh_fac][:description] + " #{data[0]} - #{data[1]}"
    # This is the version number used by framework during build and at runtime to locate, link and load the assemblies. When you add reference to any assembly in your project, it is this version number which gets embedded.
    asm.version = VERSION
    # Assembly File Version : This is the version number given to file as in file system. It is displayed by Windows Explorer. Its never used by .NET framework or runtime for referencing.
    asm.file_version = VERSION_INFORMAL
    asm.custom_attributes :AssemblyInformationalVersion => "#{VERSION}", # disposed as product version in explorer
      :CLSCompliantAttribute => false,
      :AssemblyConfiguration => "#{CONFIGURATION}",
      :Guid => Projects[:nh_fac][:guid]
    asm.com_visible = false
    asm.copyright = Projects[:nh_fac][:copyright]
    asm.output_file = 'src/CommonAssemblyInfo.cs'
  end  
  
  #                    OUTPUTTING
  # ===================================================
  task :output => [:nh_fac_output] do
    Dir.glob(File.join(Folders[:binaries], "*.txt")){ | fn | File.delete(fn) } # remove old commit marker files
	data = commit_data() # get semantic data
    File.open File.join(Folders[:binaries], "#{data[0]} - #{data[1]}.txt"), "w" do |f|
      f.puts %Q{aa
    This file's name gives you the specifics of the commit.
    
    Commit hash:		#{data[0]}
    Commit date:		#{data[1]}
}
    end
  end
  
  task :nh_fac_output => :msbuild do
    target = File.join(Folders[:binaries], Projects[:nh_fac][:dir])
    copy_files Folders[:nh_fac_out], "*.{xml,dll,pdb,config}", target
    CLEAN.include(target)
  end
  
  
  #                     TESTING
  # ===================================================
  directory "#{Folders[:tests]}"
  
  task :test => [:msbuild, "#{Folders[:tests]}", :nh_fac_nunit, :nh_fac_test_publish_artifacts]
  
  nunit :nh_fac_nunit do |nunit|
    nunit.command = Commands[:nunit]
    nunit.options '/framework v4.0', "/out #{Files[:nh_fac][:test_log]}", "/xml #{Files[:nh_fac][:test_xml]}"
    nunit.assemblies Files[:nh_fac][:test]
	CLEAN.include(Folders[:tests])
  end
  
  task :nh_fac_test_publish_artifacts => :nh_fac_nunit do
	puts "##teamcity[importData type='nunit' path='#{Files[:nh_fac][:test_xml]}']"
	puts "##teamcity[publishArtifacts '#{Files[:nh_fac][:test_log]}']"
  end

  #                      NUSPEC
  # ===================================================
  
  # copy from the key's data using the glob pattern
  def nuspec_copy(key, glob)
    puts "key: #{key}, glob: #{glob}, proj dir: #{Projects[key][:dir]}"
    FileList[File.join(Folders[:binaries], Projects[key][:dir], glob)].collect{ |f|
      to = File.join( Folders[:nuspec], "lib", FRAMEWORK )
      FileUtils.mkdir_p to
      cp f, to
	  # return the file name and its extension:
	  File.join(FRAMEWORK, File.basename(f))
    }
  end
  
  file "#{Files[:nh_fac][:nuspec]}"
  
  desc "create the nuget package"
  nuspec :nuspec do |nuspec|
    nuspec.id = Projects[:nh_fac][:id]
    nuspec.version = VERSION
    nuspec.authors = Projects[:nh_fac][:authors]
    nuspec.description = Projects[:nh_fac][:description]
    nuspec.title = Projects[:nh_fac][:title]
    nuspec.projectUrl = "https://github.com/haf/Castle.Facilities.NHibernate"
    nuspec.language = "en-US"
    nuspec.licenseUrl = "https://github.com/haf/Castle.Facilities.NHibernate/raw/develop/License.txt"
    nuspec.requireLicenseAcceptance = "true"
    nuspec.dependency "Castle.Core", "2.5.2"
    nuspec.dependency "Castle.Windsor", "2.5.2"
    nuspec.dependency "Castle.Services.Transaction", "3.0.0.1002"
    nuspec.dependency "Castle.Facilities.AutoTx", "3.0.0.1002"
	nuspec.dependency "log4net", "1.2.10"
	nuspec.dependency "FluentNHibernate", "1.2.0.712"
	nuspec.dependency "NHibernate.Castle", "3.1.0.4000"
	nuspec.framework_assembly "System.Transactions", FRAMEWORK
	
    nuspec.output_file = Files[:nh_fac][:nuspec]
	
    nuspec_copy(:nh_fac, "*Facilities.NHibernate.{dll,xml,pdb}")
	
    CLEAN.include(Folders[:nuspec])
  end
  
  #                       NUGET
  # ===================================================
  
  directory "#{Folders[:nuget]}"
  
  # creates directory tasks for all nuspec-convention based directories
  def nuget_directory()
    dirs = FileList.new([:lib, :content, :tools].collect{ |dir|
      File.join(Folders[:nuspec], "#{dir}")
    })
    task :nuget_dirs => dirs # NOTE: here a new dynamic task is defined
	dirs.to_a.each{ |d| directory d }
  end
  
  nuget_directory()
  
  desc "generate nuget package for NHibernate Facility"
  nugetpack :nuget_inner => [:output, :nuspec, "#{Folders[:nuget]}", :nuget_dirs] do |nuget|
    nuget.command     = Commands[:nuget]
    nuget.nuspec      = Files[:nh_fac][:nuspec]
    nuget.output      = Folders[:nuget]
  end
  
end