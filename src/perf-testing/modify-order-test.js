import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import uuid4 from './uuid4.js'

const client = new grpc.Client();
client.load(['../shared/protos', '../backend/OrderBookService'], 'orderbook.proto');

export default () => {
    const address = "orderbook:80";
    
    client.connect(address, {
        plaintext: true
    });

    const data = {
        idempotencyKey: {
            value: uuid4()
        },
        assetDefinition: {
            class: 'OnionFutures',
            symbol: 'TESLA'
        },
        amount: {
            nanos: '12',
            units: Math.floor(Math.random() * 1000)
        },
        price: {
            nanos: '20',
            units: Math.floor(Math.random() * 1000)
        },
        orderAction: 'Sell',
        orderId: {
            value: 'bd4e8c80-bb51-4aff-a6ba-7e5afb131ec0'
        }
    }

    const response = client.invoke('orderBook.OrderBookService/ModifyOrder', data);

    check(response, {
        'status is OK': (r) => r && r.status === grpc.StatusOK,
    });


    console.log(JSON.stringify(response.message));


    client.close();

    sleep(1);

}
