using UnityEngine;

public static class AdvandcedGizmos {
    public static void DrawWireCube (Vector3 position, Quaternion rotation, Vector3 size, Color color) {
        Vector3[] points = new Vector3[8];
        points[0] = rotation * new Vector3(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f) + position;
        points[1] = rotation * new Vector3(size.x * -0.5f, size.y * 0.5f, size.z * 0.5f) + position;
        points[2] = rotation * new Vector3(size.x * -0.5f, size.y * -0.5f, size.z * 0.5f) + position;
        points[3] = rotation * new Vector3(size.x * 0.5f, size.y * -0.5f, size.z * 0.5f) + position;

        points[4] = rotation * new Vector3(size.x * 0.5f, size.y * 0.5f, size.z * -0.5f) + position;
        points[5] = rotation * new Vector3(size.x * -0.5f, size.y * 0.5f, size.z * -0.5f) + position;
        points[6] = rotation * new Vector3(size.x * -0.5f, size.y * -0.5f, size.z * -0.5f) + position;
        points[7] = rotation * new Vector3(size.x * 0.5f, size.y * -0.5f, size.z * -0.5f) + position;

        int[] lineSegements = new int[24];
        lineSegements[0] = 0; lineSegements[1] = 1;
        lineSegements[2] = 2; lineSegements[3] = 3;
        lineSegements[4] = 4; lineSegements[5] = 5;
        lineSegements[6] = 6; lineSegements[7] = 7;

        lineSegements[8] = 2; lineSegements[9] = 1;
        lineSegements[10] = 3; lineSegements[11] = 0;
        lineSegements[12] = 6; lineSegements[13] = 5;
        lineSegements[14] = 7; lineSegements[15] = 4;

        lineSegements[16] = 2; lineSegements[17] = 6;
        lineSegements[18] = 1; lineSegements[19] = 5;
        lineSegements[20] = 3; lineSegements[21] = 7;
        lineSegements[22] = 0; lineSegements[23] = 4;

        Color backup = color;
        Gizmos.color = backup;

        for (int beg = 0, end = lineSegements.Length; beg != end; beg += 2) {
            Gizmos.DrawLine(points[lineSegements[beg]], points[lineSegements[beg + 1]]);
        }

        Gizmos.color = backup;
    }

    public static void DrawWireRoundCube (Vector3 position, Quaternion rotation, Vector3 size, float radius, Color color) {
        if (size.x * 0.5f > radius) DrawWireCube(position, rotation, size - new Vector3(0f, 2f * radius, 2f * radius), color);
        if (size.y * 0.5f > radius) DrawWireCube(position, rotation, size - new Vector3(2f * radius, 0f, 2f * radius), color);
        if (size.z * 0.5f > radius) DrawWireCube(position, rotation, size - new Vector3(2f * radius, 2f * radius, 0f), color);

        if (radius <= 0f) return;

        Color backup = color;
        Gizmos.color = backup;

        for (int beg = 0, end = 8; beg != end; ++beg) {
            float x = (beg & 0x01) != 0 ? 1f : -1f;
            float y = (beg & 0x02) != 0 ? 1f : -1f;
            float z = (beg & 0x04) != 0 ? 1f : -1f;

            float posX = size.x * 0.5f > radius ? x * (size.x * 0.5f - radius) : 0f;
            float posY = size.y * 0.5f > radius ? y * (size.y * 0.5f - radius) : 0f;
            float posZ = size.z * 0.5f > radius ? z * (size.z * 0.5f - radius) : 0f;

            Vector3 point = new Vector3(posX, posY, posZ);
            point = rotation * point + position;
            Gizmos.DrawWireSphere(point, radius);
        }

        Gizmos.color = backup;
    }

    public static void DrawWireSphere (Vector3 position, float radius, Color color) {
        Color backup = color;
        Gizmos.color = backup;

        Gizmos.DrawWireSphere(position, radius);

        Gizmos.color = backup;
    }

    public static void DrawWireCapsule (Vector3 position, Quaternion rotation, Vector3 size, Color color) {
        float radius = size.x * 0.5f;
        float height = size.y - 2f * radius;
        Vector3 circlePointA = rotation * (height * 0.5f * Vector3.up);
        Vector3 circlePointB = rotation * (height * 0.5f * Vector3.down);
        Vector3[] cylinderSegements = new Vector3[8];

        cylinderSegements[0] = (height * 0.5f * Vector3.up) + Vector3.right * radius;
        cylinderSegements[1] = (height * 0.5f * Vector3.down) + Vector3.right * radius;

        cylinderSegements[2] = (height * 0.5f * Vector3.up) + Vector3.left * radius;
        cylinderSegements[3] = (height * 0.5f * Vector3.down) + Vector3.left * radius;

        cylinderSegements[4] = (height * 0.5f * Vector3.up) + Vector3.forward * radius;
        cylinderSegements[5] = (height * 0.5f * Vector3.down) + Vector3.forward * radius;

        cylinderSegements[6] = (height * 0.5f * Vector3.up) + Vector3.back * radius;
        cylinderSegements[7] = (height * 0.5f * Vector3.down) + Vector3.back * radius;

        for (int beg = 0, end = 8; beg != end; ++beg) {
            cylinderSegements[beg] = rotation * cylinderSegements[beg] + position;
        }

        Color backup = color;
        Gizmos.color = backup;

        Gizmos.DrawWireSphere(circlePointA + position, radius);
        Gizmos.DrawWireSphere(circlePointB + position, radius);

        for(int beg = 0, end = 8; beg != end; beg += 2) {
            Gizmos.DrawLine(cylinderSegements[beg], cylinderSegements[beg + 1]);
        }

        Gizmos.color = backup;
    }
}
