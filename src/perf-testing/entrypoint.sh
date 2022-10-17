mongoimport --uri mongodb://$MONGO_USER:$MONGO_PASS@$MONGO_HOST:$MONGO_PORT/OrderBooks --collection OnionFutures --type json --file ./onion-futures-seed.json --authenticationDatabase admin

k6 run --vus 10 --duration 30s $TEST_FILE.js
