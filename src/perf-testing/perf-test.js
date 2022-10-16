import http from "k6/http";
import { sleep, check } from "k6";
import addOrderTest from "./add-order-test.js";
import getPriceTest from "./get-price-test.js";
import modifyOrderTest from "./modify-order-test.js";
//import cycleTest from "./cycle-test.js";

export default function() {
    addOrderTest();
    // getPriceTest();
    // modifyOrderTest();
    // cycleTest();
};
