# this comes after the new version got copied to /tmp/schulungsportal2
cp -r /tmp/schulungsportal /var/www/schulungsportal2/publish/
# this specific sudo command is allowed in the sudoers file, restart the service
sudo /usr/sbin/service dotnet-schulungsportal restart