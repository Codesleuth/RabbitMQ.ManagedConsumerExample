# RabbitMQ ManagedConsumer Example
This project demonstrates a pattern to delegate a custom message consumer for handling message delivery with RabbitMQ.

## Requirements
You will need to have RabbitMQ running for this example to work.
Create an exchange named "MyExchange", and ensure the default "guest" account with password "guest" exists, and has permission to create and bind queues to the exchange.
