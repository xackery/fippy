#!/usr/bin/perl

###########################################################
#::: General EQEmu Server Administration Script
#::: Purpose - Handles:
#::: 	Automatic database versioning (bots and normal DB)
#::: 	Updating server assets (binary, opcodes, maps, configuration files)
#::: Original Author: Akkadius
#::: 	Contributors: Uleat, Ali
#::: Purpose: To upgrade databases with ease and maintain versioning
###########################################################

use Config;
use File::Copy qw(copy);
use POSIX qw(strftime);
use File::Path;
use File::Find;
use Time::HiRes qw(usleep);

#############################################
# variables
#############################################
my $install_repository_request_url = "https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/";
my $eqemu_repository_request_url   = "https://raw.githubusercontent.com/EQEmu/Server/master/";
my $opcodes_path                   = "";
my $patches_path                   = "";
my $time_stamp                     = strftime('%m-%d-%Y', gmtime());
my $db_run_stage                   = 0; #::: Sets database run stage check
my $bin_dir                        = "";

#############################################
# os
#############################################
my $OS        = "";
my $os_flavor = "";
if ($Config{osname} =~ /freebsd|linux/i) {
    $OS        = "Linux";
    $os_flavor = "";
    if (-e "/etc/debian_version") {
        $os_flavor = "debian";
    }
    elsif (-e "/etc/fedora-release") {
        $os_flavor = "fedora_core";
    }
    elsif (-e "/etc/redhat-release") {
        $os_flavor = "red_hat";
    }
}
if ($Config{osname} =~ /Win|MS/i) {
    $OS = "Windows";
}

if (-e "../eqemu_config.json") {
    print "[Info] Config is up one level, let's set current directory up one level...\n";
    chdir("../");
}

#############################################
# internet check
#############################################
my $has_internet_connection = check_internet_connection();
if (-e "skip_internet_connection_check.txt") {
    $has_internet_connection = 1;
}

#############################################
# skip self update
#############################################
my $skip_self_update_check = 0;
if (-e "eqemu_server_skip_update.txt" || defined($ENV{'EQEMU_SERVER_SKIP_UPDATE'})) {
    print "[Info] Skipping self check\n";
    $skip_self_update_check = 1;
}

#############################################
# skip maps update
#############################################
my $skip_self_maps_update_check = 0;
if (-e "eqemu_server_skip_maps_update.txt" || defined($ENV{'EQEMU_SERVER_SKIP_MAPS_UPDATE'})) {
    print "[Info] Skipping maps update\n";
    $skip_self_maps_update_check = 1;
}

#############################################
# bin dir
#############################################
if (-d "bin") {
    $bin_dir = "bin/";
}

my $world_path = get_world_path();

#############################################
# run routines
#############################################
get_windows_wget();
do_self_update_check_routine() if !$skip_self_update_check;
get_perl_version();
if (-e "eqemu_config.json") {
    read_eqemu_config_json();
}
get_mysql_path();

#::: Remove old eqemu_update.pl
if (-e "eqemu_update.pl") {
    unlink("eqemu_update.pl");
}

print "[Info] For EQEmu Server management utilities - run eqemu_server.pl\n" if $ARGV[0] eq "ran_from_world";

my $skip_checks = 0;
if ($ARGV[0] && ($ARGV[0] eq "new_server" || $ARGV[0] eq "new_server_with_bots")) {
    $skip_checks = 1;
}

if ($skip_checks == 0) {
    check_db_version_table();

    #::: Check if db_version table exists...
    if (trim(get_mysql_result("SHOW COLUMNS FROM db_version LIKE 'Revision'")) ne "" && $db) {
        print get_mysql_result("DROP TABLE db_version");
        print "[Database] Old db_version table present, dropping...\n\n";
    }
}

check_for_world_bootup_database_update();


sub urlencode
{
    my ($rv) = @_;
    $rv =~ s/([^A-Za-z0-9])/sprintf("%%%2.2X", ord($1)) /ge;
    return $rv;
}

sub urldecode
{
    my ($rv) = @_;
    $rv =~ s/\+/ /g;
    $rv =~ s/%(..)/pack("c", hex($1)) /ge;
    return $rv;
}

sub do_installer_routines
{
    $build_options = $_[0];
    print "[Install] EQEmu Server Installer... LOADING... PLEASE WAIT...\n";

    #::: Make some local server directories...
    mkdir('logs');
    mkdir('updates_staged');
    mkdir('shared');
    mkdir('bin');

    $bin_dir = "bin/";

    do_install_config_json();
    read_eqemu_config_json();
    get_installation_variables();

    $db_name = "peq";
    if ($installation_variables{"mysql_eqemu_db_name"}) {
        $db_name = $installation_variables{"mysql_eqemu_db_name"};
    }

    #::: Download assets
    if ($OS eq "Windows") {
        fetch_latest_windows_appveyor();
    }

    map_files_fetch_bulk() if !$skip_self_maps_update_check;
    opcodes_fetch();
    plugins_fetch();
    quest_files_fetch();
    lua_modules_fetch();
    fetch_utility_scripts();

    #::: Database Routines
    $root_user     = $user;
    $root_password = $pass;
    print "[Database] Creating Database '" . $db_name . "'\n";
    if (defined($ENV{'MYSQL_ROOT_PASSWORD'})) {
        # In the case that the user doesn't have privileges to create databases, support passing in the root password during setup
        print "[Database] Using 'root' for database management.\n";
        $root_user     = "root";
        $root_password = $ENV{'MYSQL_ROOT_PASSWORD'};
    }
    print `"$path" --host $host --user $root_user --password="$root_password" -N -B -e "DROP DATABASE IF EXISTS $db_name;"`;
    print `"$path" --host $host --user $root_user --password="$root_password" -N -B -e "CREATE DATABASE $db_name"`;
    if (defined($ENV{'MYSQL_ROOT_PASSWORD'})) {
        # If we used root, make sure $user has permissions on db
        print "[Database] Assigning ALL PRIVILEGES to $user on $db_name.\n";
        print `"$path" --host $host --user $root_user --password="$root_password" -N -B -e "GRANT ALL PRIVILEGES ON $db_name.* TO '$user.%'"`;
        print `"$path" --host $host --user $root_user --password="$root_password" -N -B -e "FLUSH PRIVILEGES"`;
    }

    #::: Get Binary DB version
    if ($OS eq "Windows") {
        @db_version = split(': ', `"$world_path" db_version`);
    }
    if ($OS eq "Linux") {
        @db_version = split(': ', `./$world_path db_version`);
    }

    $binary_database_version = trim($db_version[1]);

    #::: Local DB Version
    check_db_version_table();
    $local_database_version = trim(get_mysql_result("SELECT version FROM db_version LIMIT 1"));

    #::: Download PEQ latest
    fetch_peq_db_full();
    print "[Database] Fetching and Applying Latest Database Updates...\n";
    main_db_management();

    # if bots
    if ($build_options =~ /bots/i) {
        bots_db_management();
    }

    remove_duplicate_rule_values();

    if ($OS eq "Windows") {
        check_windows_firewall_rules();
        do_windows_login_server_setup();
    }
    if ($OS eq "Linux") {
        do_linux_login_server_setup($build_options);
    }
}

sub check_for_input
{
    print "[Input] " . $_[0];
    $input = <STDIN>;
    chomp $input;
}

sub check_for_world_bootup_database_update
{
    $binary_database_version = 0;
    $local_database_version  = 0;

    # Usually hit during installer when world hasn't been installed yet...
    if (-e $world_path) {
        if ($OS eq "Windows") {
            @db_version = split(': ', `"$world_path" db_version`);
        }
        if ($OS eq "Linux") {
            @db_version = split(': ', `./$world_path db_version`);
        }

        $binary_database_version = trim($db_version[1]);
        $local_database_version  = get_main_db_version();
    }

    if ($binary_database_version == $local_database_version && $ARGV[0] eq "ran_from_world") {
        print "[Update] Database up to date...\n";
        if (trim($db_version[2]) == 0) {
            print "[Update] Continuing bootup\n";
            exit;
        }
    }
    else {
        #::: We ran world - Database needs to update, lets backup and run updates and continue world bootup
        if ($local_database_version < $binary_database_version && $ARGV[0] eq "ran_from_world") {
            print "[Update] Database not up to date with binaries... Automatically updating...\n";
            print "[Update] Issuing database backup first...\n";
            database_dump_compress();
            $db_already_backed_up = 1;
            print "[Update] Updating database...\n";
            sleep(1);
            main_db_management();
        }

        #::: Make sure that we didn't pass any arugments to the script
        else {
            if ($local_database_version > $binary_database_version) {
                print "[Update] Database version is ahead of current binaries...\n";
            }

            if (!$db) { print "[eqemu_server.pl] No database connection found... Running without\n"; }
            show_menu_prompt();
        }
    }

    #::: Bots
    $binary_database_version = trim($db_version[2]);
    if ($binary_database_version > 0) {
        $local_database_version = get_bots_db_version();

        #::: We ran world - Database needs to update, lets backup and run updates and continue world bootup
        if ($binary_database_version == $local_database_version && $ARGV[0] eq "ran_from_world") {
            print "[Update] Bots database up to date...\n";
        }
        else {
            if ($local_database_version < $binary_database_version && $ARGV[0] eq "ran_from_world") {
                print "[Update] Bots Database not up to date with binaries... Automatically updating...\n";
                if (!$db_already_backed_up) {
                    print "[Update] Issuing database backup first...\n";
                    database_dump_compress();
                }
                print "[Update] Updating bots database...\n";
                sleep(1);
                bots_db_management();
            }

            #::: Make sure that we didn't pass any arugments to the script
            else {
                if ($local_database_version > $binary_database_version) {
                    print "[Update] Bots database version is ahead of current binaries...\n";
                }

                if (!$db) { print "[eqemu_server.pl] No database connection found... Running without\n"; }
                show_menu_prompt();
            }
        }
    }

    print "[Update] Continuing bootup\n";
}

sub check_internet_connection
{
    if ($OS eq "Linux") {
        $count = "c";
    }
    if ($OS eq "Windows") {
        $count = "n";
    }

    if (`ping 8.8.8.8 -$count 1 -w 500` =~ /TTL|1 received/i) {
        # print "[Update] We have a connection to the internet, continuing...\n";
        return 1;
    }
    elsif (`ping 4.2.2.2 -$count 1 -w 500` =~ /TTL|1 received/i) {
        # print "[Update] We have a connection to the internet, continuing...\n";
        return 1;
    }
    else {
        print "[Update] No connection to the internet, can't check update\n";
        return;
    }
}

sub get_perl_version
{
    #::: Check Perl version
    $perl_version = $^V;
    $perl_version =~ s/v//g;
    print "[Update] Perl Version is " . $perl_version . "\n" if $debug;
    if ($perl_version > 5.12) {
        no warnings 'uninitialized';
    }
    no warnings;
}

sub get_windows_wget
{
    if (!-e "bin/wget.exe" && $OS eq "Windows") {
        if (!-d "bin") {
            mkdir("bin");
        }
        eval "use LWP::Simple qw(getstore);";
        getstore("https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/windows/wget.exe", "bin\\wget.exe");
    }
}

sub do_self_update_check_routine
{

    #::: Check for internet connection before updating
    if (!$has_internet_connection) {
        print "[Update] Cannot check self-update without internet connection...\n";
        return;
    }

    #::: Check for script changes :: eqemu_server.pl
    get_remote_file($eqemu_repository_request_url . "utils/scripts/eqemu_server.pl",
        "updates_staged/eqemu_server.pl",
        0,
        1,
        1
    );

    if (-e "updates_staged/eqemu_server.pl") {

        my $remote_script_size = -s "updates_staged/eqemu_server.pl";
        my $local_script_size  = -s "eqemu_server.pl";

        if ($remote_script_size != $local_script_size) {
            print "[Update] Script has been updated, updating...\n";

            my @files;
            my $start_dir = "updates_staged/";
            find(
                sub { push @files, $File::Find::name unless -d; },
                $start_dir
            );
            for my $file (@files) {
                if ($file =~ /eqemu_server/i) {
                    $destination_file = $file;
                    $destination_file =~ s/updates_staged\///g;
                    print "[Install] Installing [" . $destination_file . "]\n";
                    unlink($destination_file);
                    copy_file($file, $destination_file);
                    if ($OS eq "Linux") {
                        system("chmod 755 eqemu_server.pl");
                    }
                    exec("perl eqemu_server.pl ran_from_world");
                }
            }
            print "[Install] Done\n";
        }
        else {
            print "[Update] No script update necessary...\n";

            if (-e "db_update") {
                unlink("db_update");
            }

            if (-e "updates_staged") {
                unlink("updates_staged");
            }
        }

        unlink("updates_staged/eqemu_server.pl");
        unlink("updates_staged");
    }
}

sub get_installation_variables
{
    # Read installation variables from the ENV if set, but override them with install_variables.txt
    if ($ENV{"MYSQL_HOST"}) { $installation_variables{"mysql_host"}               = $ENV{"MYSQL_HOST"}; }
    if ($ENV{"MYSQL_DATABASE"}) { $installation_variables{"mysql_eqemu_db_name"}  = $ENV{"MYSQL_DATABASE"}; }
    if ($ENV{"MYSQL_USER"}) { $installation_variables{"mysql_eqemu_user"}         = $ENV{"MYSQL_USER"} }
    if ($ENV{"MYSQL_PASSWORD"}) { $installation_variables{"mysql_eqemu_password"} = $ENV{"MYSQL_PASSWORD"} }

    #::: Fetch installation variables before building the config
    if ($OS eq "Linux") {
        if (-e "../install_variables.txt") {
            open(INSTALL_VARS, "../install_variables.txt");
        }
        elsif (-e "install_variables.txt") {
            open(INSTALL_VARS, "./install_variables.txt");
        }
    }
    if ($OS eq "Windows") {
        open(INSTALL_VARS, "install_variables.txt");
    }
    while (<INSTALL_VARS>) {
        chomp;
        $o                                      = $_;
        @data                                   = split(":", $o);
        $installation_variables{trim($data[0])} = trim($data[1]);
    }
    close(INSTALL_VARS);
}

sub do_install_config_json
{
    get_installation_variables();

    #::: Fetch json template
    get_remote_file($install_repository_request_url . "eqemu_config.json", "eqemu_config_template.json");

    use JSON;
    my $json = new JSON();

    my $content;
    open(my $fh, '<', "eqemu_config_template.json") or die "cannot open file $filename"; {
    local $/;
    $content = <$fh>;
}
    close($fh);

    $config = $json->decode($content);

    $long_name                               = "Akkas " . $OS . " PEQ Installer (" . generate_random_password(5) . ')';
    $config->{"server"}{"world"}{"longname"} = $long_name;
    $config->{"server"}{"world"}{"key"}      = generate_random_password(30);

    if ($installation_variables{"mysql_eqemu_db_name"}) {
        $db_name = $installation_variables{"mysql_eqemu_db_name"};
    }
    else {
        $db_name = "peq";
    }

    if ($installation_variables{"mysql_host"}) {
        $host = $installation_variables{"mysql_host"};
    }
    else {
        $host = "127.0.0.1";
    }

    $config->{"server"}{"database"}{"host"}       = $host;
    $config->{"server"}{"database"}{"username"}   = $installation_variables{"mysql_eqemu_user"};
    $config->{"server"}{"database"}{"password"}   = $installation_variables{"mysql_eqemu_password"};
    $config->{"server"}{"database"}{"db"}         = $db_name;
    $config->{"server"}{"qsdatabase"}{"host"}     = $host;
    $config->{"server"}{"qsdatabase"}{"username"} = $installation_variables{"mysql_eqemu_user"};
    $config->{"server"}{"qsdatabase"}{"password"} = $installation_variables{"mysql_eqemu_password"};
    $config->{"server"}{"qsdatabase"}{"db"}       = $db_name;

    $json->canonical(1);

    open(my $fh, '>', 'eqemu_config.json');
    print $fh $json->pretty->utf8->encode($config);
    close $fh;

    unlink("eqemu_config_template.json");
}

sub do_install_config_login_json
{
    get_installation_variables();

    #::: Fetch json template
    get_remote_file($eqemu_repository_request_url . "loginserver/login_util/login.json", "login_template.json");

    use JSON;
    my $json = new JSON();

    my $content;
    open(my $fh, '<', "login_template.json") or die "cannot open file $filename"; {
    local $/;
    $content = <$fh>;
}
    close($fh);

    $config = $json->decode($content);

    if ($installation_variables{"mysql_eqemu_db_name"}) {
        $db_name = $installation_variables{"mysql_eqemu_db_name"};
    }
    else {
        $db_name = "peq";
    }

    if ($installation_variables{"mysql_host"}) {
        $host = $installation_variables{"mysql_host"};
    }
    else {
        $host = "127.0.0.1";
    }

    $config->{"database"}{"host"}                         = $host;
    $config->{"database"}{"user"}                         = $installation_variables{"mysql_eqemu_user"};
    $config->{"database"}{"password"}                     = $installation_variables{"mysql_eqemu_password"};
    $config->{"database"}{"db"}                           = $db_name;
    $config->{"client_configuration"}{"titanium_opcodes"} = $opcodes_path . "login_opcodes.conf";
    $config->{"client_configuration"}{"sod_opcodes"}      = $opcodes_path . "login_opcodes_sod.conf";

    $json->canonical(1);

    open(my $fh, '>', 'login.json');
    print $fh $json->pretty->utf8->encode($config);
    close $fh;

    unlink("login_template.json");
}

sub fetch_utility_scripts
{
    if ($OS eq "Windows") {

        opendir(DIR, "bin/");
        my @files = grep(/\.exe$/, readdir(DIR));
        closedir(DIR);

        foreach my $file (@files) {
            my $full_file = "bin/" . $file;

            if ($file =~ /test|launch/i) {
                next;
            }

            print "Creating Symbolic Link for [$file] from [$full_file]\n";
            system("del start_$file >nul 2>&1");
            system("powershell.exe \"New-Item -ItemType SymbolicLink -Name 'start_$file' -Value '$full_file'\" >nul 2>&1");
        }

        get_remote_file($install_repository_request_url . "windows/t_database_backup.bat", "t_database_backup.bat");
        get_remote_file($install_repository_request_url . "windows/t_start_server.bat", "t_start_server.bat");
        get_remote_file($install_repository_request_url . "windows/t_server_update_binaries_no_bots.bat",
            "t_server_update_binaries_no_bots.bat");
        get_remote_file($install_repository_request_url . "windows/t_start_server_with_login_server.bat",
            "t_start_server_with_login_server.bat");
        get_remote_file($install_repository_request_url . "windows/t_stop_server.bat", "t_stop_server.bat");
        get_remote_file($install_repository_request_url . "windows/t_server_crash_report.pl", "t_server_crash_report.pl");
        get_remote_file($install_repository_request_url . "windows/win_server_launcher.pl", "win_server_launcher.pl");
        get_remote_file($install_repository_request_url . "windows/t_start_server_with_login_server.bat",
            "t_start_server_with_login_server.bat");
        get_remote_file(
            $install_repository_request_url . "windows/t_set_gm_account.bat",
            "t_set_gm_account.bat"
        );
        get_remote_file(
            $install_repository_request_url . "windows/windows_server_readme.html",
            "windows_server_readme.html"
        );
    }
    else {
        get_remote_file($install_repository_request_url . "linux/server_launcher.pl", "server_launcher.pl");
        get_remote_file($install_repository_request_url . "linux/server_start.sh", "server_start.sh");
        get_remote_file($install_repository_request_url . "linux/server_start_dev.sh", "server_start_dev.sh");
        get_remote_file($install_repository_request_url . "linux/server_status.sh", "server_status.sh");
        get_remote_file($install_repository_request_url . "linux/server_stop.sh", "server_stop.sh");
    }
}

sub show_menu_prompt
{

    $dc = 0;
    while (1) {

        if ($ARGV[0] ne "") {
            $input = trim($ARGV[0]);
        }
        else {
            $input = trim($input);
        }

        $errored_command = 0;

        if ($input eq "check_db_updates") {
            main_db_management();
            $dc = 1;
        }
        elsif ($input eq "check_bot_db_updates") {
            bots_db_management();
            $dc = 1;
        }
        elsif ($input eq "setup_loginserver") {
            do_windows_login_server_setup();
            $dc = 1;
        }        
        elsif ($input eq "source_peq_db") {
            source_peq_db();
            $dc = 1;
        }
        elsif ($input eq "exit") {
            exit;
        }        

        #::: Errored command checking
        if ($errored_command == 1) {
            $input = $last_menu;
        }
        elsif ($dc == 1) {
            $dc    = 0;
            $input = "";
        }
        else {
            $input = <>;
        }

        #::: If we're processing a CLI command, kill the loop
        if ($ARGV[0] ne "") {
            $input   = "";
            $ARGV[0] = "";
            exit;
        }
    }
}

sub get_mysql_path
{
    if ($OS eq "Windows") {
        $has_mysql_path = `echo %PATH%`;
        if ($has_mysql_path =~ /MySQL|MariaDB/i) {
            @mysql = split(';', $has_mysql_path);
            foreach my $v (@mysql) {
                if ($v =~ /MySQL|MariaDB/i) {
                    $v =~ s/\n//g;
                    $path = trim($v) . "/mysql";
                    last;
                }
            }
        }
    }
    if ($OS eq "Linux") {
        $path = `which mysql`;
        if ($path eq "") {
            $path = `which mariadb`;
        }
        $path =~ s/\n//g;
    }

    #::: Path not found, error and exit
    if ($path eq "") {
        print "[Error:eqemu_server.pl] MySQL path not found, please add the path for automatic database upgrading to continue... \n\n";
        exit;
    }
}

sub get_world_path
{
    if (-e "world") {
        return "world";
    }
    elsif (-e "world.exe") {
        return "world.exe";
    }
    elsif (-e "bin/world") {
        return "bin/world";
    }
    elsif (-e "bin/world.exe") {
        return "bin/world.exe";
    }
}

sub get_world_command
{
    my $command    = "";
    my $world_path = get_world_path();
    if ($OS eq "Windows") {
        $command = "\"$world_path\"";
    }
    if ($OS eq "Linux") {
        $command = "./$world_path";
    }

    return $command;
}

sub database_dump
{
    print "[Database] Performing database backup....\n";
    my $command = get_world_command();
    print `$command database:dump --all`;
}

sub database_dump_player_tables
{
    print "[Database] Performing database backup of player tables....\n";
    my $command = get_world_command();
    print `$command database:dump --player-tables`;
}

sub database_dump_compress
{
    print "[Database] Performing database backup....\n";
    my $command = get_world_command();
    print `$command database:dump --all --compress`;
}

sub script_exit
{
    #::: Cleanup staged folder...
    rmtree("updates_staged/");
    exit;
}

sub check_db_version_table
{
    if (get_mysql_result("SHOW TABLES LIKE 'db_version'") eq "" && $db) {
        print "[Database] Table 'db_version' does not exist.... Creating...\n\n";
        print get_mysql_result("
			CREATE TABLE db_version (
			  version int(11) DEFAULT '0'
			) ENGINE=InnoDB DEFAULT CHARSET=latin1;
			INSERT INTO db_version (version) VALUES ('1000');");
    }
}

#::: Returns Tab Delimited MySQL Result from Command Line
sub get_mysql_result
{
    my $run_query = $_[0];
    if (!$db) { return; }
    if ($OS eq "Windows") { return `"$path" --host $host --user $user --password="$pass" $db -N -B -e "$run_query"`; }
    if ($OS eq "Linux") {
        $run_query =~ s/`//g;
        return `$path --user="$user" --host $host --password="$pass" $db -N -B -e "$run_query"`;
    }
}

sub get_mysql_result_from_file
{
    my $update_file = $_[0];
    if (!$db) {
        return;
    }

    if ($OS eq "Windows") {
        return `"$path" --host $host --user $user --password="$pass" --force $db < $update_file`;
    }

    if ($OS eq "Linux") {
        return `"$path" --host $host --user $user --password="$pass" --force $db < $update_file`;
    }
}

#::: Gets Remote File based on request_url (1st Arg), and saves to destination file (2nd Arg)
#::: Example: get_remote_file($eqemu_repository_request_url . "utils/sql/db_update_manifest.txt", "db_update/db_update_manifest.txt");
sub get_remote_file
{
    my $request_url      = $_[0];
    my $destination_file = $_[1];
    my $content_type     = $_[2];
    my $no_retry         = $_[3];
    my $silent_download  = $_[4];

    if (!$has_internet_connection) {
        print "[Download] Cannot download without internet connection...\n";
        return;
    }

    #::: Build file path of the destination file so that we may check for the folder's existence and make it if necessary

    if ($destination_file =~ /\//i) {
        my @directory_path = split('/', $destination_file);
        $build_path        = "";
        $directory_index   = 0;
        while ($directory_path[$directory_index] && $directory_path[$directory_index + 1]) {
            $build_path .= $directory_path[$directory_index] . "/";
            # print "checking '" . $build_path . "'\n";
            #::: If path does not exist, create the directory...
            if (!-d $build_path) {
                print "[Copy] folder doesn't exist, creating [" . $build_path . "]\n";
                mkdir($build_path);
            }
            if (!$directory_indexr_path[$directory_index + 2] && $directory_indexr_path[$directory_index + 1]) {
                # print $actual_path . "\n";
                $actual_path = $build_path;
                last;
            }
            $directory_index++;
        }
    }

    #::: wget -O db_update/db_update_manifest.txt https://raw.githubusercontent.com/EQEmu/Server/master/utils/sql/db_update_manifest.txt
    if ($OS eq "Linux") {
        $wget = `wget -N --no-cache --cache=no --no-check-certificate --quiet -O $destination_file $request_url`;
    }
    elsif ($OS eq "Windows") {
        $wget = `bin\\wget.exe -N --no-cache --cache=no --no-check-certificate --quiet -O $destination_file $request_url`;
    }
    print "[Download] Saved [" . $destination_file . "] from [" . $request_url . "]\n" if !$silent_download;
    if ($wget =~ /unable to resolve/i) {
        print "Error, no connection or failed request...\n\n";
        #die;
    }

}

#::: Trim Whitespaces
sub trim
{
    my $string = $_[0];
    $string =~ s/^\s+//;
    $string =~ s/\s+$//;
    return $string;
}

sub read_eqemu_config_json
{
    use JSON;
    my $json = new JSON();

    my $content;
    open(my $fh, '<', "eqemu_config.json") or die "cannot open file $filename"; {
    local $/;
    $content = <$fh>;
}
    close($fh);

    $config = $json->decode($content);

    $db           = $config->{"server"}{"database"}{"db"};
    $host         = $config->{"server"}{"database"}{"host"};
    $user         = $config->{"server"}{"database"}{"username"};
    $pass         = $config->{"server"}{"database"}{"password"};
    $opcodes_path = $config->{"server"}{"directories"}{"opcodes"};
    $patches_path = $config->{"server"}{"directories"}{"patches"};
}

#::: Fetch Latest PEQ AA's
sub aa_fetch
{
    if (!$db) {
        print "No database present, check your eqemu_config.json for proper MySQL/MariaDB configuration...\n";
        return;
    }

    print "[Install] Pulling down PEQ AA Tables...\n";
    get_remote_file($eqemu_repository_request_url . "utils/sql/peq_aa_tables_post_rework.sql",
        "db_update/peq_aa_tables_post_rework.sql");
    print "[Install] Installing AA Tables...\n";
    print get_mysql_result_from_file("db_update/peq_aa_tables_post_rework.sql");
    print "[Install] Done...\n\n";
}

sub remove_duplicate_rule_values
{
    $ruleset_id = trim(get_mysql_result("SELECT `ruleset_id` FROM `rule_sets` WHERE `name` = 'default'"));
    print "[Database] Default Ruleset ID: " . $ruleset_id . "\n";

    $total_removed = 0;

    #::: Store Default values...
    $mysql_result = get_mysql_result("SELECT * FROM `rule_values` WHERE `ruleset_id` = " . $ruleset_id);
    my @lines     = split("\n", $mysql_result);
    foreach my $val (@lines) {
        my @values                      = split("\t", $val);
        $rule_set_values{$values[1]}[0] = $values[2];
    }

    #::: Compare default values against other rulesets to check for duplicates...
    $mysql_result = get_mysql_result("SELECT * FROM `rule_values` WHERE `ruleset_id` != " . $ruleset_id);
    my @lines     = split("\n", $mysql_result);
    foreach my $val (@lines) {
        my @values = split("\t", $val);
        if ($values[2] == $rule_set_values{$values[1]}[0]) {
            print "[Database] Removing duplicate : " . $values[1] . " (Ruleset (" . $values[0] . ")) matches default value of : " . $values[2] . "\n";
            get_mysql_result("DELETE FROM `rule_values` WHERE `ruleset_id` = " . $values[0] . " AND `rule_name` = '" . $values[1] . "'");
            $total_removed++;
        }
    }

    print "[Database] Total duplicate rules removed... " . $total_removed . "\n";
}

sub copy_file
{
    $l_source_file      = $_[0];
    $l_destination_file = $_[1];

    if ($l_destination_file =~ /\//i) {
        my @directory_path = split('/', $l_destination_file);
        $build_path        = "";
        $directory_index   = 0;
        while ($directory_path[$directory_index]) {
            $build_path .= $directory_path[$directory_index] . "/";
            #::: If path does not exist, create the directory...
            if (!-d $build_path) {
                mkdir($build_path);
            }
            if (!$directory_path[$directory_index + 2] && $directory_path[$directory_index + 1]) {
                # print $actual_path . "\n";
                $actual_path = $build_path;
                last;
            }
            $directory_index++;
        }
    }

    copy $l_source_file, $l_destination_file;
}

sub do_windows_login_server_setup
{
    print "[Install] Pulling down Loginserver database tables...\n";
    get_remote_file(
        $eqemu_repository_request_url . "loginserver/login_util/login_schema.sql",
        "db_update/login_schema.sql"
    );

    get_remote_file(
        $eqemu_repository_request_url . "loginserver/login_util/login_opcodes_sod.conf",
        $opcodes_path . "login_opcodes_sod.conf"
    );

    get_remote_file(
        $eqemu_repository_request_url . "loginserver/login_util/login_opcodes.conf",
        $opcodes_path . "login_opcodes.conf"
    );

    print "[Install] Installing Loginserver tables...\n";
    print get_mysql_result_from_file("db_update/login_schema.sql");
    print "[Install] Done...\n";

    print "[Install] Pulling and initializing Loginserver configuration files...\n";
    do_install_config_login_json();
    print "[Install] Done...\n";

    add_login_server_firewall_rules();

    rmtree('updates_staged');
    rmtree('db_update');

    print "[Install] Press any key to continue...\n";

    <>; #Read from STDIN

}

sub do_linux_login_server_setup
{
    build_linux_source($_[0]);

    for my $file (@files) {
        $destination_file = $file;
        $destination_file =~ s/updates_staged\/login_server\///g;
        print "[Install] Installing [" . $destination_file . "]\n";
        copy_file($file, $destination_file);
    }
    print "\n Done... \n";

    print "[Install] Pulling down Loginserver database tables...\n";
    get_remote_file($eqemu_repository_request_url . "loginserver/login_util/login_schema.sql",
        "db_update/login_schema.sql");
    print "[Install] Installing Loginserver tables...\n";
    print get_mysql_result_from_file("db_update/login_schema.sql");
    print "[Install] Done...\n\n";

    print "[Install] Pulling and initializing Loginserver configuration files...\n";
    do_install_config_login_json();
    print "[Install] Done...\n";

    rmtree('updates_staged');
    rmtree('db_update');

    get_remote_file($install_repository_request_url . "linux/login_opcodes.conf", $opcodes_path . "login_opcodes.conf");
    get_remote_file($install_repository_request_url . "linux/login_opcodes_sod.conf",
        $opcodes_path . "login_opcodes_sod.conf");
    get_remote_file($install_repository_request_url . "linux/server_start_with_login.sh", "server_start_with_login.sh");
    system("chmod 755 *.sh");

    print "[Install] Press any key to continue...\n";

    <>; #Read from STDIN

}

sub add_login_server_firewall_rules
{
    #::: Check Loginserver Firewall install for Windows
    if ($OS eq "Windows") {
        $output                         = `netsh advfirewall firewall show rule name=all`;
        @output_buffer                  = split("\n", $output);
        $has_loginserver_rules_titanium = 0;
        $has_loginserver_rules_sod      = 0;
        foreach my $val (@output_buffer) {
            if ($val =~ /Rule Name/i) {
                $val =~ s/Rule Name://g;
                if ($val =~ /EQEmu Loginserver/i && $val =~ /Titanium/i) {
                    $has_loginserver_rules_titanium = 1;
                    print "[Install] Found existing rule [" . trim($val) . "]\n";
                }
                if ($val =~ /EQEmu Loginserver/i && $val =~ /SOD/i) {
                    $has_loginserver_rules_sod = 1;
                    print "[Install] Found existing rule [" . trim($val) . "]\n";
                }
            }
        }

        if ($has_loginserver_rules_titanium == 0) {
            print "[Install] Attempting to add EQEmu Loginserver Firewall Rules (Titanium) (TCP) port 5998 \n";
            print `netsh advfirewall firewall add rule name="EQEmu Loginserver (Titanium) (5998) TCP" dir=in action=allow protocol=TCP localport=5998`;
            print "[Install] Attempting to add EQEmu Loginserver Firewall Rules (Titanium) (UDP) port 5998 \n";
            print `netsh advfirewall firewall add rule name="EQEmu Loginserver (Titanium) (5998) UDP" dir=in action=allow protocol=UDP localport=5998`;
        }
        if ($has_loginserver_rules_sod == 0) {
            print "[Install] Attempting to add EQEmu Loginserver Firewall Rules (SOD+) (TCP) port 5999 \n";
            print `netsh advfirewall firewall add rule name="EQEmu Loginserver (SOD+) (5999) TCP" dir=in action=allow protocol=TCP localport=5999`;
            print "[Install] Attempting to add EQEmu Loginserver Firewall Rules (SOD+) (UDP) port 5999 \n";
            print `netsh advfirewall firewall add rule name="EQEmu Loginserver (SOD+) (5999) UDP" dir=in action=allow protocol=UDP localport=5999`;
        }

        print "If firewall rules don't add you must run this script (eqemu_server.pl) as administrator\n";
        print "\n";
        print "[Install] Instructions \n";
        print "[Install] In order to connect your server to the loginserver you must point your eqemu_config.json to your local server similar to the following:\n";
        print "
	\"loginserver1\" : {
		\"account\" : \"\",
		\"host\" : \"login.eqemulator.net\",
		\"password\" : \"\",
		\"port\" : \"5998\",
		\"legacy\": \"1\"
	},
	\"loginserver2\" : {
		\"account\" : \"\",
		\"host\" : \"192.168.197.129\",
		\"password\" : \"\",
		\"port\" : \"5998\"
	},
	\"localaddress\" : \"192.168.197.129\",
		";
        print "[Install] When done, make sure your EverQuest client points to your loginserver's IP (In this case it would be 192.168.197.129) in the eqhosts.txt file\n";
    }
}

sub check_windows_firewall_rules
{
    $output          = `netsh advfirewall firewall show rule name=all`;
    @output_buffer   = split("\n", $output);
    $has_world_rules = 0;
    $has_zone_rules  = 0;
    foreach my $val (@output_buffer) {
        if ($val =~ /Rule Name/i) {
            $val =~ s/Rule Name://g;
            if ($val =~ /EQEmu World/i) {
                $has_world_rules = 1;
                print "[Install] Found existing rule [" . trim($val) . "]\n";
            }
            if ($val =~ /EQEmu Zone/i) {
                $has_zone_rules = 1;
                print "[Install] Found existing rule [" . trim($val) . "]\n";
            }
        }
    }

    if ($has_world_rules == 0) {
        print "[Install] Attempting to add EQEmu World Firewall Rules (TCP) port 9000 \n";
        print `netsh advfirewall firewall add rule name="EQEmu World (9000) TCP" dir=in action=allow protocol=TCP localport=9000`;
        print "[Install] Attempting to add EQEmu World Firewall Rules (UDP) port 9000 \n";
        print `netsh advfirewall firewall add rule name="EQEmu World (9000) UDP" dir=in action=allow protocol=UDP localport=9000`;
    }
    if ($has_zone_rules == 0) {
        print "[Install] Attempting to add EQEmu Zones (7000-7500) TCP \n";
        print `netsh advfirewall firewall add rule name="EQEmu Zones (7000-7500) TCP" dir=in action=allow protocol=TCP localport=7000-7500`;
        print "[Install] Attempting to add EQEmu Zones (7000-7500) UDP \n";
        print `netsh advfirewall firewall add rule name="EQEmu Zones (7000-7500) UDP" dir=in action=allow protocol=UDP localport=7000-7500`;
    }
}

sub fetch_server_dlls
{
    # print "[Download] Fetching lua51.dll, zlib1.dll, zlib1.pdb, libmysql.dll...\n";
    # get_remote_file($install_repository_request_url . "lua51.dll", "lua51.dll", 1);
    # get_remote_file($install_repository_request_url . "zlib1.dll", "zlib1.dll", 1);
    # get_remote_file($install_repository_request_url . "zlib1.pdb", "zlib1.pdb", 1);
    # get_remote_file($install_repository_request_url . "libmysql.dll", "libmysql.dll", 1);
}

sub source_peq_db
{    
    my $start_dir = "../cache/peq-db-latest/peq-dump";
    find(
        sub { push @files, $File::Find::name unless -d; },
        $start_dir
    );
    for my $file (@files) {
        $destination_file = $file;
        $destination_file =~ s/updates_staged\/peq_db\/peq-dump\///g;
        if ($file =~ /create_tables_content|create_tables_login|create_tables_player|create_tables_queryserv|create_tables_state|create_tables_system/i) {
            print "[Install] Database sourcing [" . $destination_file . "]\n";
            get_mysql_result_from_file($file);
        }
    }
}

sub do_file_diff
{
    $file_1 = $_[0];
    $file_2 = $_[1];
    if ($OS eq "Windows") {
        eval "use Text::Diff";
        $directory_indexff = diff($file_1, $file_2, { STYLE => "Unified" });
        return $directory_indexff;
    }
    if ($OS eq "Linux") {
        # print 'diff -u "$file_1" "$file_2"' . "\n";
        return `diff -u "$file_1" "$file_2"`;
    }
}

sub unzip
{
    $archive_to_unzip = $_[0];
    $dest_folder      = $_[1];

    if ($OS eq "Windows") {
        eval "use Archive::Zip qw( :ERROR_CODES :CONSTANTS )";
        my $zip = Archive::Zip->new();
        unless ($zip->read($archive_to_unzip) == AZ_OK) {
            die 'read error';
        }
        print "[Unzip] Extracting...\n";
        $zip->extractTree('', $dest_folder);
    }
    if ($OS eq "Linux") {
        print `unzip -o -q "$archive_to_unzip" -d "$dest_folder"`;
    }
}

sub are_file_sizes_different
{
    $file_1    = $_[0];
    $file_2    = $_[1];
    my $file_1 = (stat $file_1)[7];
    my $file_2 = (stat $file_2)[7];
    # print $file_1 . " :: " . $file_2 . "\n";
    if ($file_1 != $file_2) {
        return 1;
    }
    return;
}

sub do_bots_db_schema_drop
{
    #"drop_bots.sql" is run before reverting database back to 'normal'
    print "[Database] Fetching drop_bots.sql...\n";
    get_remote_file($eqemu_repository_request_url . "utils/sql/git/bots/drop_bots.sql", "db_update/drop_bots.sql");
    print get_mysql_result_from_file("db_update/drop_bots.sql");

    print "[Database] Removing bot database tables...\n";

    if (get_mysql_result("SHOW KEYS FROM `group_id` WHERE `Key_name` LIKE 'PRIMARY'") ne "" && $db) {
        print get_mysql_result("ALTER TABLE `group_id` DROP PRIMARY KEY;");
    }
    print get_mysql_result("ALTER TABLE `group_id` ADD PRIMARY KEY (`groupid`, `charid`, `ismerc`);");

    if (get_mysql_result("SHOW KEYS FROM `guild_members` WHERE `Key_name` LIKE 'PRIMARY'") ne "" && $db) {
        print get_mysql_result("ALTER TABLE `guild_members` DROP PRIMARY KEY;");
    }
    print get_mysql_result("ALTER TABLE `guild_members` ADD PRIMARY KEY (`char_id`);");

    print get_mysql_result("UPDATE `spawn2` SET `enabled` = 0 WHERE `id` IN (59297,59298);");

    if (get_mysql_result("SHOW COLUMNS FROM `db_version` LIKE 'bots_version'") ne "" && $db) {
        print get_mysql_result("UPDATE `db_version` SET `bots_version` = 0;");
    }
    print "[Database] Done...\n";
}

sub modify_db_for_bots
{
    #Called after the db bots schema (2015_09_30_bots.sql) has been loaded
    print "[Database] Modifying database for bots...\n";
    print get_mysql_result("UPDATE `spawn2` SET `enabled` = 1 WHERE `id` IN (59297,59298);");

    if (get_mysql_result("SHOW KEYS FROM `guild_members` WHERE `Key_name` LIKE 'PRIMARY'") ne "" && $db) {
        print get_mysql_result("ALTER TABLE `guild_members` DROP PRIMARY KEY;");
    }

    if (get_mysql_result("SHOW KEYS FROM `group_id` WHERE `Key_name` LIKE 'PRIMARY'") ne "" && $db) {
        print get_mysql_result("ALTER TABLE `group_id` DROP PRIMARY KEY;");
    }
    print get_mysql_result("ALTER TABLE `group_id` ADD PRIMARY KEY USING BTREE(`groupid`, `charid`, `name`, `ismerc`);");

    convert_existing_bot_data();
}

sub convert_existing_bot_data
{
    if (get_mysql_result("SHOW TABLES LIKE 'bots'") ne "" && $db) {
        print "[Database] Converting existing bot data...\n";
        print get_mysql_result("INSERT INTO `bot_data` (`bot_id`, `owner_id`, `spells_id`, `name`, `last_name`, `zone_id`, `gender`, `race`, `class`, `level`, `creation_day`, `last_spawn`, `time_spawned`, `size`, `face`, `hair_color`, `hair_style`, `beard`, `beard_color`, `eye_color_1`, `eye_color_2`, `drakkin_heritage`, `drakkin_tattoo`, `drakkin_details`, `ac`, `atk`, `hp`, `mana`, `str`, `sta`, `cha`, `dex`, `int`, `agi`, `wis`, `fire`, `cold`, `magic`, `poison`, `disease`, `corruption`) SELECT `BotID`, `BotOwnerCharacterID`, `BotSpellsID`, `Name`, `LastName`, `LastZoneId`, `Gender`, `Race`, `Class`, `BotLevel`, UNIX_TIMESTAMP(`BotCreateDate`), UNIX_TIMESTAMP(`LastSpawnDate`), `TotalPlayTime`, `Size`, `Face`, `LuclinHairColor`, `LuclinHairStyle`, `LuclinBeard`, `LuclinBeardColor`, `LuclinEyeColor`, `LuclinEyeColor2`, `DrakkinHeritage`, `DrakkinTattoo`, `DrakkinDetails`, `AC`, `ATK`, `HP`, `Mana`, `STR`, `STA`, `CHA`, `DEX`, `_INT`, `AGI`, `WIS`, `FR`, `CR`, `MR`, `PR`, `DR`, `Corrup` FROM `bots`;");

        print get_mysql_result("INSERT INTO `bot_inspect_messages` (`bot_id`, `inspect_message`) SELECT `BotID`, `BotInspectMessage` FROM `bots`;");

        print get_mysql_result("RENAME TABLE `bots` TO `bots_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botstances'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_stances` (`bot_id`, `stance_id`) SELECT bs.`BotID`, bs.`StanceID` FROM `botstances` bs INNER JOIN `bot_data` bd ON bs.`BotID` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `botstances` TO `botstances_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'bottimers'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_timers` (`bot_id`, `timer_id`, `timer_value`) SELECT bt.`BotID`, bt.`TimerID`, bt.`Value` FROM `bottimers` bt INNER JOIN `bot_data` bd ON bt.`BotID` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `bottimers` TO `bottimers_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botbuffs'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_buffs` (`buffs_index`, `bot_id`, `spell_id`, `caster_level`, `duration_formula`, `tics_remaining`, `poison_counters`, `disease_counters`, `curse_counters`, `corruption_counters`, `numhits`, `melee_rune`, `magic_rune`, `persistent`) SELECT bb.`BotBuffId`, bb.`BotId`, bb.`SpellId`, bb.`CasterLevel`, bb.`DurationFormula`, bb.`TicsRemaining`, bb.`PoisonCounters`, bb.`DiseaseCounters`, bb.`CurseCounters`, bb.`CorruptionCounters`, bb.`HitCount`, bb.`MeleeRune`, bb.`MagicRune`, bb.`Persistent` FROM `botbuffs` bb INNER JOIN `bot_data` bd ON bb.`BotId` = bd.`bot_id`;");

        if (get_mysql_result("SHOW COLUMNS FROM `botbuffs` LIKE 'dot_rune'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_buffs` bb INNER JOIN `botbuffs` bbo ON bb.`buffs_index` = bbo.`BotBuffId` SET bb.`dot_rune` = bbo.`dot_rune` WHERE bb.`bot_id` = bbo.`BotID`;");
        }

        if (get_mysql_result("SHOW COLUMNS FROM `botbuffs` LIKE 'caston_x'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_buffs` bb INNER JOIN `botbuffs` bbo ON bb.`buffs_index` = bbo.`BotBuffId` SET bb.`caston_x` = bbo.`caston_x` WHERE bb.`bot_id` = bbo.`BotID`;");
        }

        if (get_mysql_result("SHOW COLUMNS FROM `botbuffs` LIKE 'caston_y'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_buffs` bb INNER JOIN `botbuffs` bbo ON bb.`buffs_index` = bbo.`BotBuffId` SET bb.`caston_y` = bbo.`caston_y` WHERE bb.`bot_id` = bbo.`BotID`;");
        }

        if (get_mysql_result("SHOW COLUMNS FROM `botbuffs` LIKE 'caston_z'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_buffs` bb INNER JOIN `botbuffs` bbo ON bb.`buffs_index` = bbo.`BotBuffId` SET bb.`caston_z` = bbo.`caston_z` WHERE bb.`bot_id` = bbo.`BotID`;");
        }

        if (get_mysql_result("SHOW COLUMNS FROM `botbuffs` LIKE 'ExtraDIChance'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_buffs` bb INNER JOIN `botbuffs` bbo ON bb.`buffs_index` = bbo.`BotBuffId` SET bb.`extra_di_chance` = bbo.`ExtraDIChance` WHERE bb.`bot_id` = bbo.`BotID`;");
        }

        print get_mysql_result("RENAME TABLE `botbuffs` TO `botbuffs_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botinventory'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_inventories` (`inventories_index`, `bot_id`, `slot_id`, `item_id`, `inst_charges`, `inst_color`, `inst_no_drop`, `augment_1`, `augment_2`, `augment_3`, `augment_4`, `augment_5`) SELECT bi.`BotInventoryID`, bi.`BotID`, bi.`SlotID`, bi.`ItemID`, bi.`charges`, bi.`color`, bi.`instnodrop`, bi.`augslot1`, bi.`augslot2`, bi.`augslot3`, bi.`augslot4`, bi.`augslot5` FROM `botinventory` bi INNER JOIN `bot_data` bd ON bi.`BotID` = bd.`bot_id`;");

        if (get_mysql_result("SHOW COLUMNS FROM `botinventory` LIKE 'augslot6'") ne "" && $db) {
            print get_mysql_result("UPDATE `bot_inventories` bi INNER JOIN `botinventory` bio ON bi.`inventories_index` = bio.`BotInventoryID` SET bi.`augment_6` = bio.`augslot6` 	WHERE bi.`bot_id` = bio.`BotID`;");
        }

        print get_mysql_result("RENAME TABLE `botinventory` TO `botinventory_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botpets'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_pets` (`pets_index`, `pet_id`, `bot_id`, `name`, `mana`, `hp`) SELECT bp.`BotPetsId`, bp.`PetId`, bp.`BotId`, bp.`Name`, bp.`Mana`, bp.`HitPoints` FROM `botpets` bp INNER JOIN `bot_data` bd ON bp.`BotId` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `botpets` TO `botpets_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botpetbuffs'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_pet_buffs` (`pet_buffs_index`, `pets_index`, `spell_id`, `caster_level`, `duration`) SELECT bpb.`BotPetBuffId`, bpb.`BotPetsId`, bpb.`SpellId`, bpb.`CasterLevel`, bpb.`Duration` FROM `botpetbuffs` bpb INNER JOIN `bot_pets` bp ON bpb.`BotPetsId` = bp.`pets_index`;");

        print get_mysql_result("RENAME TABLE `botpetbuffs` TO `botpetbuffs_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botpetinventory'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_pet_inventories` (`pet_inventories_index`, `pets_index`, `item_id`) SELECT bpi.`BotPetInventoryId`, bpi.`BotPetsId`, bpi.`ItemId` FROM `botpetinventory` bpi INNER JOIN `bot_pets` bp ON bpi.`BotPetsId` = bp.`pets_index`;");

        print get_mysql_result("RENAME TABLE `botpetinventory` TO `botpetinventory_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botgroup'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_groups` (`groups_index`, `group_leader_id`, `group_name`) SELECT bg.`BotGroupId`, bg.`BotGroupLeaderBotId`, bg.`BotGroupName` FROM  `botgroup` bg INNER JOIN `bot_data` bd ON bg.`BotGroupLeaderBotId` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `botgroup` TO `botgroup_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botgroupmembers'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_group_members` (`group_members_index`, `groups_index`, `bot_id`) SELECT bgm.`BotGroupMemberId`, bgm.`BotGroupId`, bgm.`BotId` FROM `botgroupmembers` bgm INNER JOIN `bot_groups` bg ON bgm.`BotGroupId` = bg.`groups_index` INNER JOIN `bot_data` bd ON bgm.`BotId` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `botgroupmembers` TO `botgroupmembers_old`;");
    }

    if (get_mysql_result("SHOW TABLES LIKE 'botguildmembers'") ne "" && $db) {
        print get_mysql_result("INSERT INTO `bot_guild_members` (`bot_id`, `guild_id`, `rank`, `tribute_enable`, `total_tribute`, `last_tribute`, `banker`, `public_note`, `alt`) SELECT bgm.`char_id`, bgm.`guild_id`, bgm.`rank`, bgm.`tribute_enable`, bgm.`total_tribute`, bgm.`last_tribute`, bgm.`banker`, bgm.`public_note`, bgm.`alt` FROM `botguildmembers` bgm INNER JOIN `guilds` g ON bgm.`guild_id` = g.`id` INNER JOIN `bot_data` bd ON bgm.`char_id` = bd.`bot_id`;");

        print get_mysql_result("RENAME TABLE `botguildmembers` TO `botguildmembers_old`;");
    }
}

sub get_main_db_version
{
    $main_local_db_version = trim(get_mysql_result("SELECT version FROM db_version LIMIT 1"));
    return $main_local_db_version;
}

sub get_bots_db_version
{
    #::: Check if bots_version column exists...
    if (get_mysql_result("SHOW COLUMNS FROM db_version LIKE 'bots_version'") eq "" && $db) {
        print get_mysql_result("ALTER TABLE db_version ADD bots_version int(11) DEFAULT '0' AFTER version;");
        print "[Database] Column 'bots_version' does not exists.... Adding to 'db_version' table...\n\n";
    }

    $bots_local_db_version = trim(get_mysql_result("SELECT bots_version FROM db_version LIMIT 1"));
    return $bots_local_db_version;
}

#::: Safe for call from world startup or menu option
sub bots_db_management
{
    #::: If we have stale data from main db run
    if ($db_run_stage > 0 && $bots_db_management == 0) {
        clear_database_runs();
    }

    #::: Main Binary Database version
    $binary_database_version = trim($db_version[2]);
    if ($binary_database_version == 0) {
        print "[Database] Your server binaries (world/zone) are not compiled for bots...\n\n";
        return;
    }
    $local_database_version = get_bots_db_version();

    #::: Set on flag for running bot updates...
    $bots_db_management = 1;

    if ($local_database_version > $binary_database_version) {
        print "[Update] Bots database version is ahead of current binaries...\n";
        return;
    }

    run_database_check();
}

#::: Safe for call from world startup or menu option
sub main_db_management
{
    #::: If we have stale data from bots db run
    if ($db_run_stage > 0 && $bots_db_management == 1) {
        clear_database_runs();
    }

    #::: Main Binary Database version
    $binary_database_version = trim($db_version[1]);
    $local_database_version  = get_main_db_version();

    $bots_db_management = 0;

    if ($local_database_version > $binary_database_version) {
        print "[Update] Database version is ahead of current binaries...\n";
        return;
    }

    run_database_check();
}

sub clear_database_runs
{
    # print "DEBUG :: clear_database_runs\n\n";
    #::: Clear manifest data...
    %m_d = ();
    #::: Clear updates...
    @total_updates = ();
}

#::: Responsible for Database Upgrade Routines
sub run_database_check
{

    if (!$db) {
        print "No database present, check your eqemu_config.json for proper MySQL/MariaDB configuration...\n";
        return;
    }

    #::: Pull down bots database manifest
    if ($bots_db_management == 1) {
        print "[Database] Retrieving latest bots database manifest...\n";
        get_remote_file($eqemu_repository_request_url . "utils/sql/git/bots/bots_db_update_manifest.txt",
            "db_update/db_update_manifest.txt");
    }
    #::: Pull down mainstream database manifest
    else {
        print "[Database] Retrieving latest database manifest...\n";
        get_remote_file($eqemu_repository_request_url . "utils/sql/db_update_manifest.txt",
            "db_update/db_update_manifest.txt");
    }

    #::: Parse manifest
    print "[Database] Reading manifest...\n";

    use Data::Dumper;
    open(FILE, "db_update/db_update_manifest.txt");
    while (<FILE>) {
        chomp;
        $o = $_;
        if ($o =~ /#/i) {
            next;
        }

        @manifest          = split('\|', $o);
        $m_d{$manifest[0]} = [ @manifest ];
    }

    #::: This is where we set checkpoints for where a database might be so we don't check so far back in the manifest...
    if ($local_database_version >= 9000) {
        $revision_check = $local_database_version + 1;
    }
    else {
        #::: This does not negatively affect bots
        $revision_check = 1000;
        if (get_mysql_result("SHOW TABLES LIKE 'character_data'") ne "") {
            $revision_check = 8999;
        }
    }

    @total_updates = ();

    #::: Fetch and register sqls for this database update cycle
    for ($i = $revision_check; $i <= $binary_database_version; $i++) {
        if (!defined($m_d{$i}[0])) {
            next;
        }

        $file_name = trim($m_d{$i}[1]);
        print "[Database] fetching update: " . $i . " '" . $file_name . "' \n";
        fetch_missing_db_update($i, $file_name);
        push(@total_updates, $i);
    }

    if (scalar(@total_updates) == 0) {
        print "[Database] No updates need to be run...\n";
        if ($bots_db_management == 1) {
            print "[Database] Setting Database to Bots Binary Version (" . $binary_database_version . ") if not already...\n\n";
            get_mysql_result("UPDATE db_version SET bots_version = $binary_database_version ");
        }
        else {
            print "[Database] Setting Database to Binary Version (" . $binary_database_version . ") if not already...\n\n";
            get_mysql_result("UPDATE db_version SET version = $binary_database_version ");
        }

        clear_database_runs();
        return;
    }

    #::: Execute pending updates
    @total_updates = sort @total_updates;
    foreach my $val (@total_updates) {
        $file_name   = trim($m_d{$val}[1]);
        $query_check = trim($m_d{$val}[2]);
        $match_type  = trim($m_d{$val}[3]);
        $match_text  = trim($m_d{$val}[4]);

        #::: Match type update
        if ($match_type eq "contains") {
            if (trim(get_mysql_result($query_check)) =~ /$match_text/i) {
                print "[Database] Applying update [" . $val . "]:[" . $file_name . "]\n";
                print get_mysql_result_from_file("db_update/$file_name");
            }
            else {
                print "[Database] Has update [" . $val . "]:[" . $file_name . "]\n";
            }
            print_match_debug();
            print_break();
        }
        if ($match_type eq "missing") {
            if (get_mysql_result($query_check) =~ /$match_text/i) {
                print "[Database] Has update [" . $val . "]:[" . $file_name . "]\n";
            }
            else {
                print "[Database] Applying update [" . $val . "]:[" . $file_name . "]\n";
                print get_mysql_result_from_file("db_update/$file_name");
            }
            print_match_debug();
            print_break();
        }
        if ($match_type eq "empty") {
            if (get_mysql_result($query_check) eq "") {
                print "[Database] Applying update [" . $val . "]:[" . $file_name . "]\n";
                print get_mysql_result_from_file("db_update/$file_name");
            }
            else {
                print "[Database] Has update [" . $val . "]:[" . $file_name . "' \n";
            }
            print_match_debug();
            print_break();
        }
        if ($match_type eq "not_empty") {
            if (get_mysql_result($query_check) ne "") {
                print "[Database] Applying update [" . $val . "]:[" . $file_name . "]\n";
                print get_mysql_result_from_file("db_update/$file_name");
            }
            else {
                print "[Database] Has update [" . $val . "]:[" . $file_name . "]\n";
            }
            print_match_debug();
            print_break();
        }

        if ($bots_db_management == 1) {
            print get_mysql_result("UPDATE db_version SET bots_version = $val WHERE bots_version < $val");

            if ($val == 9000) {
                modify_db_for_bots();
            }
        }
        else {
            print get_mysql_result("UPDATE db_version SET version = $val WHERE version < $val");

            if ($val == 9138) {
                fix_quest_factions();
            }
        }
    }

    if ($bots_db_management == 1) {
        print "[Database] Bots database update cycle complete at version [" . get_bots_db_version() . "]\n";
    }
    else {
        print "[Database] Mainstream database update cycle complete at version [" . get_main_db_version() . "]\n";
    }
}

sub print_match_debug
{
    if (!$debug) { return; }
    print "	Match Type: '" . $match_type . "'\n";
    print "	Match Text: '" . $match_text . "'\n";
    print "	Query Check: '" . $query_check . "'\n";
    print "	Result: '" . trim(get_mysql_result($query_check)) . "'\n";
}

sub print_break
{
    if (!$debug) { return; }
    print "\n==============================================\n";
}
