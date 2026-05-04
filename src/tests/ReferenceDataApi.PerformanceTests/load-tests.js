import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 20 }, 
        { duration: '1m', target: 20 },  
        { duration: '30s', target: 0 } 
    ],

    thresholds: {
        http_req_duration: ['p(95)<2000'], 
        http_req_failed: ['rate<0.01']    
    }
};

const apiUrl = 'https://solid-bassoon-4q7vwq56qx792jpwq-5000.app.github.dev';
const type = 'location';
const apiKey = 'admin-secret-123';

export default function () {

    const headers = {
        'Content-Type': 'application/json',
        'x-api-key': apiKey
    };

    let payload = JSON.stringify({
        label: `k6-load-${__VU}-${__ITER}`,
        active: true,
        derivesFrom: null
    });

    let createRes = http.post(`${apiUrl}/${type}`, payload, { headers });

    check(createRes, {
        'POST succeeded': (r) => r.status === 201
    });

    let id = null;
    try {
        id = createRes.json('id');
    } catch (e) {}

    if (id) {
        let getRes = http.get(`${apiUrl}/${type}/${id}`, { headers });

        check(getRes, {
            'GET succeeded': (r) => r.status === 200
        });
    }

    if (id) {
        let updateRes = http.put(`${apiUrl}/${type}/${id}`,
            JSON.stringify({
                label: 'updated-load',
                active: false,
                derivesFrom: null
            }),
            { headers });

        check(updateRes, {
            'PUT succeeded': (r) => r.status === 204
        });
    }

    if (id) {
        let deleteRes = http.del(`${apiUrl}/${type}/${id}`, null, { headers });

        check(deleteRes, {
            'DELETE succeeded': (r) => r.status === 204
        });
    }

    sleep(1);
}