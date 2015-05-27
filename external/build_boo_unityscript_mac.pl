#!/usr/bin/perl
use warnings;
use strict;
use File::Basename qw(dirname basename fileparse);
use File::Spec;
use File::Copy;
use File::Path;

my $root = "";
my $nant = "";
my $externalDir = "";

main();

sub main {
	get_root();
	setup_nant();
	build_boo();
	build_boo_extensions();
	build_unityscript();
}

sub check_dirs {
	chdir $root;

	die ("Must grab Boo implementation") if !-d "boo";
	die ("Must grab Boo extensions") if !-d "boo-extensions";
	die ("Must grab Unityscript implementation") if !-d "unityscript";
}

sub get_root {
	$root = File::Spec->rel2abs( dirname($0) );
	chdir $root;
	$root = File::Spec->rel2abs( File::Spec->updir() );
	chdir $root;
	$root = File::Spec->rel2abs( File::Spec->updir() );
	chdir $root;

	$externalDir = "$root/MonoDevelop.Boo.UnityScript.Addins/external";
}

sub setup_nant {
	$ENV{PKG_CONFIG_PATH}="/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig";
	$nant = "mono --runtime=v4.0.30319 $externalDir/dependencies/nant-0.93-nightly-2015-02-12/bin/NAnt.exe";

}

sub build_boo {
	chdir "$root/boo";
	system("$nant rebuild") && die ("Failed to build Boo");
	mkpath "$externalDir/Boo";
	system("rsync -av --exclude=*.mdb  \"$root/boo/build/\" \"$externalDir/Boo\"");
}

sub build_boo_extensions {
	chdir "$root/boo-extensions";
#	copy "$externalDir/dependencies/build.properties", "$root/boo-extensions/build.properties";
	system("$nant rebuild") && die ("Failed to build Boo extenions");
}

sub build_unityscript {
	chdir "$root/unityscript";
	rmtree "$root/unityscript/bin";
	system("$nant rebuild") && die ("Failed to build UnityScript");
	mkpath "$externalDir/UnityScript";
	system("rsync -av --exclude=*.mdb --exclude=*Tests* --exclude=nunit* \"$root/unityscript/bin/\" \"$externalDir/UnityScript\"");
}