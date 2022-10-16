import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import uuid4 from './uuid4.js'

const client = new grpc.Client();
client.load(['../shared/protos', '../backend/OrderBookService'], 'orderbook.proto');

export default () => {

    client.connect('localhost:5237', {
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
            units: '12'
        },
        price: {
            nanos: '20',
            units: '25000'
        },
        orderAction: 'Sell',
    }

    const response = client.invoke('orderBook.OrderBookService/AddOrder', data);

    check(response, {

        'status is OK': (r) => r && r.status === grpc.StatusOK,

    });


    console.log(JSON.stringify(response.message));


    client.close();

    sleep(1);

}
