# These should probably be parameterised
# Ideally, mongo would be reinitialised between tests too

mongoimport --uri mongodb://mongo:mongo_pass@mongo:27017/OrderBooks --collection OnionFutures --type json --file ./onion-futures-seed.json --authenticationDatabase admin

k6 run --vus 10 --duration 30s get-price-test.js > /get-price-test-results.json
k6 run --vus 10 --duration 30s add-order-test.js > /add-order-test-results.json
k6 run --vus 10 --duration 30s modify-order-test.js > /modify-order-test-results.json
k6 run --vus 10 --duration 30s cycle-test.js > /cycle-test-results.json
