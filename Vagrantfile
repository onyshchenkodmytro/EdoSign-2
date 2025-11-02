Vagrant.configure("2") do |config|
  # ---------- VM 1: BaGet (NuGet сервер) ----------
  config.vm.define "baget" do |baget|
    baget.vm.box = "bento/ubuntu-22.04"
    baget.vm.hostname = "baget"

    # IP-адреса для зв'язку з іншими VM
    baget.vm.network "private_network", ip: "192.168.56.10"

    # Проброс порту (щоб можна було зайти з хоста)
    baget.vm.network "forwarded_port", guest: 5555, host: 5555, auto_correct: true

    baget.vm.provision "shell", inline: <<-SHELL
      apt-get update -y
      apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release docker.io

      docker run -d --restart=always --name baget \
        -e "ASPNETCORE_URLS=http://0.0.0.0:80" \
        -e "Baget__ApiKey=secret" \
        -e "Baget__Storage__Type=FileSystem" \
        -e "Baget__Storage__Path=/var/baget/packages" \
        -e "Baget__Database__Type=Sqlite" \
        -e "Baget__Database__ConnectionString=Data Source=/var/baget/baget.db" \
        -p 5555:80 \
        -v /var/baget:/var/baget \
        loicsharma/baget:latest

      echo "BaGet running on http://192.168.56.10:5555/v3/index.json"
    SHELL
  end

  # ---------- VM 2: Ubuntu (клієнт) ----------
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "bento/ubuntu-22.04"
    ubuntu.vm.hostname = "ubuntu-client"

    #  лише проброс порту
    ubuntu.vm.network "forwarded_port", guest: 5000, host: 8081, auto_correct: true

    ubuntu.vm.provision "shell", inline: <<-SHELL
      apt-get update -y
      apt-get install -y wget git apt-transport-https xdg-utils

      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -y
      apt-get install -y dotnet-sdk-9.0

      mkdir -p /home/vagrant/.nuget/NuGet
      cat >/home/vagrant/.nuget/NuGet/NuGet.Config <<'CFG'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="baget" value="http://192.168.56.10:5555/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <baget>
      <add key="Username" value="apikey" />
      <add key="ClearTextPassword" value="secret" />
    </baget>
  </packageSourceCredentials>
</configuration>
CFG

      su - vagrant -c "rm -rf ~/EdoSign && git clone https://github.com/onyshchenkodmytro/EdoSign ~/EdoSign"
      cd /home/vagrant/EdoSign

      # Будуємо і запускаємо
      dotnet restore
      dotnet build -c Release
      dotnet publish EdoSign.Api -c Release -o /app

      echo "Starting EdoSign API..."
      nohup dotnet /app/EdoSign.Api.dll --urls=http://0.0.0.0:5000 > /var/log/edosign.log 2>&1 &

      #  чекаємо, поки сервер стартує
      sleep 10

      # автоматично відкриваємо Swagger на хості (через проброс порту)
      xdg-open "http://localhost:8081/swagger/index.html" || echo "Swagger доступний на http://localhost:8081/swagger/index.html"

      echo "API running. Log file: /var/log/edosign.log"
    SHELL
  end
end
