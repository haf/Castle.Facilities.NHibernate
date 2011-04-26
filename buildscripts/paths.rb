root_folder = File.expand_path("#{File.dirname(__FILE__)}/..")

Folders = {
  :root => root_folder,
  :src => "src",
  :out => "build",
  :tests => File.join("build", "tests"),
  :tools => "tools",
  :nunit => File.join("tools", "NUnit", "bin"),
  
  :packages => "packages",
  :nuspec => File.join("packages", Projects[:nhib_fac][:dir]),
  :nuget => File.join("build", "nuget"),
  
  :nh_fac_out => 'placeholder - specify build environment',
  :nh_fac_test_out => 'placeholder - specify build environment',
  :binaries => "placeholder - specify build environment"
}

Files = {
  :sln => "Castle.Facilities.NHibernate.sln",
  :version => "VERSION",
  
  :nh_fac => {
    :nuspec => File.join(Folders[:nuspec], "#{Projects[:nhib_fac][:dir]}.nuspec"),
	:test_log => File.join(Folders[:tests], "Castle.Facilities.NHibernate.Tests.log"),
	:test_xml => File.join(Folders[:tests], "Castle.Facilities.NHibernate.Tests.xml"),
	:test => 'ex: Castle.Facilities.NHibernate.Tests.dll'
  }
}

Commands = {
  :nunit => File.join(Folders[:nunit], "nunit-console.exe"),
  :nuget => File.join(Folders[:tools], "NuGet.exe"),
  :ilmerge => File.join(Folders[:tools], "ILMerge.exe")
}