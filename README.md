A máquina local do desenvolvedor deve configurar um dominio http://oldpocket.com:8888 para que o exemplo funcione.

O console coloca no ar um servidor que ficará esperando o callback do Masterpass.

Após iniciar o console, o desenvolvedor deve abrir a página http://oldpocket.com:8888 para iniciar o primeiro passo do express checkout, que será um checkout completo com o requestPairing = true;

A página de retorno ira mostrar o transactionId dessa transacao e o longAccessToken. Havera uma URL apontando para uma nova pagina que ira simular uma segunda compra desse consumidor, que ja possui um longAccessToken e executara um Express Checkout diratemente.


