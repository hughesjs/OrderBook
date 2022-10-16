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
        assetDefinition: {
            class: 'OnionFutures',
            symbol: 'ONION'
        },
        amount: {
            nanos: '0',
            units: '50000'
        },
        orderAction: "Buy",
    }

    const response = client.invoke('orderBook.OrderBookService/GetPrice', data);

    check(response, {

        'status is OK': (r) => r && r.status === grpc.StatusOK,

    });


    console.log(JSON.stringify(response.message));


    client.close();

    sleep(1);

}
